package com.api.dashboard.Service;

import com.api.dashboard.Dominio.RegistroViolencia;
import com.api.dashboard.Repository.CsvRepository;
import jakarta.annotation.PostConstruct;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

@Service
public class DashboardService {

    private final CsvRepository repository;
    private List<RegistroViolencia> registros;

    public DashboardService(CsvRepository repository) {
        this.repository = repository;
    }

    @PostConstruct
    public void init() {
        this.registros = repository.lerCsv();
    }

    private boolean isSim(String valor) {
        return valor != null && valor.trim().equalsIgnoreCase("Sim");
    }

    // -----------------------------------------------------------------
    // Totais e agregacoes por municipio
    // -----------------------------------------------------------------
    public int totalRegistros() {
        return registros.size();
    }

    public Map<String, Integer> casosPorMunicipio() {
        Map<String, Integer> mapa = new TreeMap<>();
        for (RegistroViolencia r : registros) {
            if (r.getMunicipio() != null) {
                mapa.merge(r.getMunicipio(), 1, Integer::sum);
            }
        }
        return mapa;
    }

    public Map<String, Integer> violenciaFisicaPorMunicipio() {
        Map<String, Integer> mapa = new TreeMap<>();
        for (RegistroViolencia r : registros) {
            if (isSim(r.getViolFisica()) && r.getMunicipio() != null) {
                mapa.merge(r.getMunicipio(), 1, Integer::sum);
            }
        }
        return mapa;
    }

    // -----------------------------------------------------------------
    // Tipos de violencia por ano
    // -----------------------------------------------------------------
    public Map<String, Map<String, Integer>> dadosViolenciaPorAno() {
        Map<String, Map<String, Integer>> tabela = new TreeMap<>();
        for (RegistroViolencia r : registros) {
            if (r.getAno() == null) continue;
            String ano = String.valueOf(r.getAno());
            Map<String, Integer> mapa = tabela.computeIfAbsent(ano, k -> new TreeMap<>());

            if (isSim(r.getViolFisica())) mapa.merge("Fisica", 1, Integer::sum);
            if (isSim(r.getViolPsico()))  mapa.merge("Psicologica", 1, Integer::sum);
            if (isSim(r.getViolTort()))   mapa.merge("Tortura", 1, Integer::sum);
            if (isSim(r.getViolSexual())) mapa.merge("Sexual", 1, Integer::sum);
        }
        return tabela;
    }

    // -----------------------------------------------------------------
    // Casos por faixa de idade
    // -----------------------------------------------------------------
    public Map<String, Integer> dadosCasosPorIdade() {
        Map<String, Integer> faixas = new LinkedHashMap<>();
        faixas.put("0-12", 0);
        faixas.put("13-17", 0);
        faixas.put("18-24", 0);
        faixas.put("25-35", 0);
        faixas.put("36-59", 0);
        faixas.put("60+", 0);

        for (RegistroViolencia r : registros) {
            Double idade = r.getIdade();
            if (idade == null) continue;

            if (idade <= 12) faixas.merge("0-12", 1, Integer::sum);
            else if (idade <= 17) faixas.merge("13-17", 1, Integer::sum);
            else if (idade <= 24) faixas.merge("18-24", 1, Integer::sum);
            else if (idade <= 35) faixas.merge("25-35", 1, Integer::sum);
            else if (idade <= 59) faixas.merge("36-59", 1, Integer::sum);
            else faixas.merge("60+", 1, Integer::sum);
        }
        return faixas;
    }

    // -----------------------------------------------------------------
    // Casos por hora do dia
    // -----------------------------------------------------------------
    public Map<Integer, Integer> dadosCasosPorHora() {
        Map<Integer, Integer> contagem = new TreeMap<>();
        for (int i = 0; i < 24; i++) contagem.put(i, 0);

        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");

        for (RegistroViolencia r : registros) {
            String data = r.getDtOcorrencia();
            int hora = 12; // padrao quando nao ha componente de horario

            try {
                if (data != null && data.length() > 10) {
                    hora = LocalDateTime.parse(data, formatter).getHour();
                }
            } catch (Exception ignored) {
                // mantem a hora padrao
            }
            contagem.merge(hora, 1, Integer::sum);
        }
        return contagem;
    }
}
