# api-ia — API de IA (predição do tipo de violência)

API em **FastAPI + scikit-learn + XGBoost** que serve os modelos treinados
sobre o questionário de risco de violência doméstica. Dado o conjunto de
respostas, retorna o **tipo de violência mais provável** e as probabilidades
por classe.

Classes possíveis: `Física`, `Moral`, `Patrimonial`, `Psicológica`, `Sexual`.

## Estrutura

```
api-ia/
├─ app/
│  ├─ main.py           # app FastAPI + endpoints
│  ├─ model_loader.py   # carrega os .pkl (joblib) uma vez (singleton)
│  ├─ schemas.py        # entrada/saída (Pydantic) + enums do questionário
│  ├─ vectorizer.py     # respostas -> vetor de 51 features (ordem do treino)
│  └─ models/           # artefatos de ML (joblib)
│     ├─ feature_names.pkl
│     ├─ label_encoder.pkl
│     ├─ rf_violencia.pkl
│     └─ xgb_violencia.pkl
├─ requirements.txt
└─ Dockerfile
```

> **Importante:** os `.pkl` foram serializados com **joblib**, então são
> carregados com `joblib.load` (não `pickle.load`). As versões em
> `requirements.txt` (scikit-learn 1.6.1, xgboost 2.1.3, numpy < 2.2) batem com
> as do treino para evitar erros de deserialização.

## Como rodar

### Docker (via compose, na raiz do projeto)

```bash
docker compose up --build api-ia
```

→ http://localhost:8000/docs

### Local (sem Docker)

```bash
cd api-ia
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload --port 8000
```

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET  | `/ia/health`        | Status, modelos carregados, nº de features |
| GET  | `/ia/features`      | Lista das 51 features e das classes |
| POST | `/ia/prever`        | Predição a partir das respostas do questionário |
| POST | `/ia/prever-vetor`  | Predição a partir de um vetor cru `{feature: 0/1}` |

### Exemplo — `POST /ia/prever`

```json
{
  "q1": "sim",
  "q2": "tiro",
  "q21": "soco",
  "q4": "ciume_controle",
  "q3_sexual": true,
  "q11_arma": true,
  "q19_cohabita": true,
  "q20_dependencia": true,
  "rel_afetiva": "Companheiro(a)",
  "rel_domiciliar": "Pessoa que reside no mesmo lar",
  "modelo": "xgb"
}
```

Resposta:

```json
{
  "modelo": "xgb",
  "classe_prevista": "Sexual",
  "confianca": 0.9963,
  "probabilidades": {
    "Física": 0.003, "Moral": 0.0003, "Patrimonial": 0.0003,
    "Psicológica": 0.0001, "Sexual": 0.9963
  }
}
```

Todos os campos do questionário são **opcionais**; quando omitidos, o one-hot
correspondente fica zerado. O campo `modelo` aceita `"rf"` (Random Forest) ou
`"xgb"` (XGBoost, padrão).

## Valores aceitos (campos categóricos)

- **q1**: `com arma de fogo`, `com faca`, `de outra forma`, `não`, `sim`
- **q2**: `afogamento`, `enforcamento`, `estrangulamento`, `facada`, `nenhuma agressão física`, `outro`, `paulada`, `queimadura`, `sufocamento`, `tiro`
- **q21**: `chute`, `empurrão`, `nenhuma agressão física`, `puxão de cabelo`, `soco`, `tapa`
- **q4**: `ameaca_nao_sera`, `ciume_controle`, `fez_telefonemas`, `impediu_acesso_dinheiro`, `nenhum_controle`, `perturbou_perseguiu`, `proibiu_trabalhar`, `proibiu_visitar`
- **rel_afetiva**: `Companheiro(a)`, `Cunhado(a)`, `Ex-companheiro(a)`, `Ex-marido ou ex-esposo(a)`, `Ex-namorado(a)`, `Filho(a)`, `Irmão(ã)`, `Marido ou Esposo(a)`, `Namorado(a)`, `Padrasto`, `Pai`, `Primo(a)`
- **rel_domiciliar**: `Ex-residente do lar`, `Outro`, `Pessoa que reside no mesmo lar`, `Pessoa que reside no mesmo lar, Ex-residente do lar`
- **flags booleanas**: `q3_sexual`, `q11_arma`, `q12_terceiros`, `q15_isolamento`, `q19_cohabita`, `q20_dependencia`
