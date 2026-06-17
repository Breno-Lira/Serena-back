"""
Carregamento dos artefatos de ML (joblib).

IMPORTANTE: os .pkl foram serializados com joblib (NumpyArrayWrapper), portanto
DEVEM ser carregados com joblib.load — pickle.load puro falha com
"STACK_GLOBAL requires str".

Versões fixadas em requirements.txt (scikit-learn==1.6.1, xgboost==2.1.3,
numpy<2.2) para casar com as versões usadas no treino e evitar
InconsistentVersionWarning / quebras de deserialização.
"""

from functools import lru_cache
from pathlib import Path

import joblib

MODELS_DIR = Path(__file__).resolve().parent / "models"


class Artefatos:
    def __init__(self) -> None:
        self.feature_names = joblib.load(MODELS_DIR / "feature_names.pkl")
        self.label_encoder = joblib.load(MODELS_DIR / "label_encoder.pkl")
        self.modelos = {
            "rf": joblib.load(MODELS_DIR / "rf_violencia.pkl"),
            "xgb": joblib.load(MODELS_DIR / "xgb_violencia.pkl"),
        }
        # Lista de classes legíveis (ex.: ['Física','Moral',...])
        self.classes = list(self.label_encoder.classes_)

    def modelo(self, nome: str):
        if nome not in self.modelos:
            raise KeyError(nome)
        return self.modelos[nome]


@lru_cache(maxsize=1)
def get_artefatos() -> Artefatos:
    """Singleton — carrega os modelos uma única vez no processo."""
    return Artefatos()
