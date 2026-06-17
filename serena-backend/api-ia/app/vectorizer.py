"""
Conversão das respostas do questionário (schema amigável) para o vetor de 51
features one-hot/binárias que os modelos esperam, na ordem correta de
`feature_names`.
"""

from typing import Dict

import pandas as pd

from .schemas import PrevisaoRequest


def _set(vetor: Dict[str, float], chave: str) -> None:
    """Marca uma feature como 1 se ela existir no vetor (ignora desconhecidas)."""
    if chave in vetor:
        vetor[chave] = 1.0


def request_para_dataframe(req: PrevisaoRequest, feature_names) -> pd.DataFrame:
    # Começa tudo zerado, na ordem exata do treino
    vetor: Dict[str, float] = {nome: 0.0 for nome in feature_names}

    # One-hots (prefixo __ ou rel_*)
    if req.q1 is not None:
        _set(vetor, f"q1__{req.q1.value}")
    if req.q2 is not None:
        _set(vetor, f"q2__{req.q2.value}")
    if req.q21 is not None:
        _set(vetor, f"q21__{req.q21.value}")
    if req.q4 is not None:
        _set(vetor, f"q4__{req.q4.value}")
    if req.rel_afetiva is not None:
        _set(vetor, f"rel_af_{req.rel_afetiva.value}")
    if req.rel_domiciliar is not None:
        _set(vetor, f"rel_dom_{req.rel_domiciliar.value}")

    # Flags binárias
    if req.q3_sexual:
        _set(vetor, "q3_sexual")
    if req.q11_arma:
        _set(vetor, "q11_arma")
    if req.q12_terceiros:
        _set(vetor, "q12_terceiros")
    if req.q15_isolamento:
        _set(vetor, "q15_isolamento")
    if req.q19_cohabita:
        _set(vetor, "q19_cohabita")
    if req.q20_dependencia:
        _set(vetor, "q20_dependencia")

    # DataFrame com colunas na ordem do treino (modelos foram treinados com nomes)
    return pd.DataFrame([[vetor[n] for n in feature_names]], columns=list(feature_names))


def dict_para_dataframe(features: Dict[str, float], feature_names) -> pd.DataFrame:
    """Para o endpoint de vetor cru: aceita um dict parcial e completa com zeros."""
    vetor = {nome: 0.0 for nome in feature_names}
    for k, v in features.items():
        if k in vetor:
            vetor[k] = float(v)
    return pd.DataFrame([[vetor[n] for n in feature_names]], columns=list(feature_names))
