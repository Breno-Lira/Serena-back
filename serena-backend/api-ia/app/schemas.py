"""
Schemas de entrada e saída da API de IA.

O modelo foi treinado com 51 features one-hot/binárias. Para não obrigar o
cliente a montar o vetor de 51 posições na mão, a API expõe um schema
"amigável" (respostas brutas do questionário) e o converte internamente para o
vetor que os modelos esperam (ver vectorizer.py).

Categorias do questionário (form_app de risco de violência doméstica):

- q1   : agressão/ameaça com arma (one-hot)
- q2   : tentativa de matar / agressão física grave (one-hot)
- q21  : agressão física (one-hot)
- q4   : controle/perseguição do agressor (one-hot)
- flags: perguntas sim/não (q3_sexual, q11_arma, q12_terceiros,
         q15_isolamento, q19_cohabita, q20_dependencia)
- rel_af  : relação afetiva/parentesco com o agressor (one-hot)
- rel_dom : relação domiciliar com o agressor (one-hot)
"""

from enum import Enum
from typing import Dict, List, Optional

from pydantic import BaseModel, Field


# --------------------------------------------------------------------------- #
# Enums — valores aceitos em cada campo categórico
# --------------------------------------------------------------------------- #
class Q1(str, Enum):
    com_arma_de_fogo = "com arma de fogo"
    com_faca = "com faca"
    de_outra_forma = "de outra forma"
    nao = "não"
    sim = "sim"


class Q2(str, Enum):
    afogamento = "afogamento"
    enforcamento = "enforcamento"
    estrangulamento = "estrangulamento"
    facada = "facada"
    nenhuma = "nenhuma agressão física"
    outro = "outro"
    paulada = "paulada"
    queimadura = "queimadura"
    sufocamento = "sufocamento"
    tiro = "tiro"


class Q21(str, Enum):
    chute = "chute"
    empurrao = "empurrão"
    nenhuma = "nenhuma agressão física"
    outro = "outro"
    puxao_cabelo = "puxão de cabelo"
    soco = "soco"
    tapa = "tapa"


class Q4(str, Enum):
    ameaca_nao_sera = "ameaca_nao_sera"
    ciume_controle = "ciume_controle"
    fez_telefonemas = "fez_telefonemas"
    impediu_acesso_dinheiro = "impediu_acesso_dinheiro"
    nenhum_controle = "nenhum_controle"
    perturbou_perseguiu = "perturbou_perseguiu"
    proibiu_trabalhar = "proibiu_trabalhar"
    proibiu_visitar = "proibiu_visitar"


class RelAfetiva(str, Enum):
    companheiro = "Companheiro(a)"
    cunhado = "Cunhado(a)"
    ex_companheiro = "Ex-companheiro(a)"
    ex_marido = "Ex-marido ou ex-esposo(a)"
    ex_namorado = "Ex-namorado(a)"
    filho = "Filho(a)"
    outro = "Outro"
    irmao = "Irmão(ã)"
    marido = "Marido ou Esposo(a)"
    namorado = "Namorado(a)"
    padrasto = "Padrasto"
    pai = "Pai"
    primo = "Primo(a)"


class RelDomiciliar(str, Enum):
    ex_residente = "Ex-residente do lar"
    outro = "Outro"
    reside_mesmo_lar = "Pessoa que reside no mesmo lar"
    reside_e_ex = "Pessoa que reside no mesmo lar, Ex-residente do lar"


class ModeloEnum(str, Enum):
    rf = "rf"
    xgb = "xgb"


# --------------------------------------------------------------------------- #
# Entrada amigável (POST /ia/prever)
# --------------------------------------------------------------------------- #
class PrevisaoRequest(BaseModel):
    """Respostas do questionário. Todos os campos são opcionais; quando omitido,
    o one-hot correspondente fica zerado (ausência da informação)."""

    q1: Optional[Q1] = Field(None, description="Agressão/ameaça com arma")
    q2: Optional[Q2] = Field(None, description="Tentativa de matar / agressão grave")
    q21: Optional[Q21] = Field(None, description="Tipo de agressão física")
    q4: Optional[Q4] = Field(None, description="Controle/perseguição do agressor")

    q3_sexual: bool = Field(False, description="Houve violência sexual")
    q11_arma: bool = Field(False, description="Agressor possui/usou arma")
    q12_terceiros: bool = Field(False, description="Ameaça a terceiros (filhos, familiares)")
    q15_isolamento: bool = Field(False, description="Vítima isolada socialmente")
    q19_cohabita: bool = Field(False, description="Coabita com o agressor")
    q20_dependencia: bool = Field(False, description="Dependência financeira do agressor")

    rel_afetiva: Optional[RelAfetiva] = Field(None, description="Vínculo afetivo/parentesco")
    rel_domiciliar: Optional[RelDomiciliar] = Field(None, description="Vínculo domiciliar")

    modelo: ModeloEnum = Field(
        ModeloEnum.xgb, description="Modelo a usar: 'rf' (Random Forest) ou 'xgb' (XGBoost)"
    )

    model_config = {
        "json_schema_extra": {
            "example": {
                "q1": "sim",
                "q2": "tiro",
                "q21": "soco",
                "q4": "ciume_controle",
                "q3_sexual": True,
                "q11_arma": True,
                "q12_terceiros": False,
                "q15_isolamento": True,
                "q19_cohabita": True,
                "q20_dependencia": True,
                "rel_afetiva": "Companheiro(a)",
                "rel_domiciliar": "Pessoa que reside no mesmo lar",
                "modelo": "xgb",
            }
        }
    }


# --------------------------------------------------------------------------- #
# Entrada crua (POST /ia/prever-vetor) — para quem já tem o vetor de features
# --------------------------------------------------------------------------- #
class VetorRequest(BaseModel):
    features: Dict[str, float] = Field(
        ...,
        description="Mapa nome_da_feature -> valor (0/1). Features ausentes viram 0.",
    )
    modelo: ModeloEnum = ModeloEnum.xgb


# --------------------------------------------------------------------------- #
# Saída
# --------------------------------------------------------------------------- #
class PrevisaoResponse(BaseModel):
    modelo: str
    classe_prevista: str = Field(..., description="Tipo de violência previsto")
    confianca: float = Field(..., description="Probabilidade da classe prevista (0-1)")
    probabilidades: Dict[str, float] = Field(..., description="Probabilidade por classe")


class FeaturesResponse(BaseModel):
    total: int
    feature_names: List[str]
    classes: List[str]
