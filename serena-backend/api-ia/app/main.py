"""
Serena — API de IA (predição do tipo de violência).

Serve os modelos Random Forest e XGBoost treinados sobre o questionário de
risco de violência doméstica. Saída: tipo de violência mais provável
(Física, Moral, Patrimonial, Psicológica, Sexual) e probabilidades por classe.

Stack: FastAPI + scikit-learn + xgboost. Porta padrão: 8000.
Documentação interativa: http://localhost:8000/docs
"""

import warnings

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware

from .model_loader import get_artefatos
from .schemas import (
    FeaturesResponse,
    PrevisaoRequest,
    PrevisaoResponse,
    VetorRequest,
)
from .vectorizer import dict_para_dataframe, request_para_dataframe

# Silencia o aviso de versão (modelos treinados em sklearn 1.6.1; fixado em requirements)
warnings.filterwarnings("ignore", category=UserWarning)

app = FastAPI(
    title="Serena — API de IA",
    description=(
        "Predição do tipo de violência a partir das respostas do questionário "
        "de risco. Modelos: Random Forest e XGBoost."
    ),
    version="1.0.0",
)

# CORS liberado (mesmo padrão das outras APIs do projeto; ajuste em produção)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.on_event("startup")
def _carregar_modelos() -> None:
    # Força o carregamento dos modelos no startup (falha rápido se algo der errado)
    get_artefatos()


def _prever(df, nome_modelo: str) -> PrevisaoResponse:
    art = get_artefatos()
    try:
        modelo = art.modelo(nome_modelo)
    except KeyError:
        raise HTTPException(status_code=400, detail=f"Modelo inválido: {nome_modelo}")

    idx = int(modelo.predict(df)[0])
    proba = modelo.predict_proba(df)[0]

    classe = art.label_encoder.inverse_transform([idx])[0]
    # label_encoder.classes_ está alinhado com as colunas de predict_proba
    probabilidades = {
        str(art.classes[i]): round(float(p), 4) for i, p in enumerate(proba)
    }

    return PrevisaoResponse(
        modelo=nome_modelo,
        classe_prevista=str(classe),
        confianca=round(float(proba[idx]), 4),
        probabilidades=probabilidades,
    )


# --------------------------------------------------------------------------- #
# Endpoints
# --------------------------------------------------------------------------- #
@app.get("/ia/health", tags=["infra"])
def health():
    art = get_artefatos()
    return {
        "status": "ok",
        "modelos": list(art.modelos.keys()),
        "n_features": len(art.feature_names),
        "classes": art.classes,
    }


@app.get("/ia/features", response_model=FeaturesResponse, tags=["infra"])
def features():
    art = get_artefatos()
    return FeaturesResponse(
        total=len(art.feature_names),
        feature_names=list(art.feature_names),
        classes=[str(c) for c in art.classes],
    )


@app.post("/ia/prever", response_model=PrevisaoResponse, tags=["predição"])
def prever(req: PrevisaoRequest):
    """Predição a partir das respostas do questionário (schema amigável)."""
    art = get_artefatos()
    df = request_para_dataframe(req, art.feature_names)
    return _prever(df, req.modelo.value)


@app.post("/ia/prever-vetor", response_model=PrevisaoResponse, tags=["predição"])
def prever_vetor(req: VetorRequest):
    """Predição a partir de um vetor cru {feature: 0/1}. Features ausentes viram 0."""
    art = get_artefatos()
    df = dict_para_dataframe(req.features, art.feature_names)
    return _prever(df, req.modelo.value)


@app.get("/", tags=["infra"])
def root():
    return {"servico": "Serena API de IA", "docs": "/docs", "health": "/ia/health"}
