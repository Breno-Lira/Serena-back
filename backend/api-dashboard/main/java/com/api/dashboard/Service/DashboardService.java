package com.api.dashboard.Service;

import org.springframework.stereotype.Service;

// MUDANÇA: Usando Jakarta em vez de javax
import jakarta.annotation.PostConstruct; 

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.*;

@Service
public class DashboardService {

    private List<String[]> linhas;

    @PostConstruct // Anotação agora é de jakarta.annotation
    public void init() {
        linhas = carregarCsv("dados.csv"); 
    }

    private List<String[]> carregarCsv(String arquivo) {
        try (BufferedReader br = new BufferedReader(
                new InputStreamReader(
                    Objects.requireNonNull(getClass().getClassLoader().getResourceAsStream(arquivo))
                )
            )) {

            return br.lines()
                    .skip(1)
                    .map(l -> l.split(","))
                    .toList();

        } catch (Exception e) {
            throw new RuntimeException("Erro ao carregar CSV: " + arquivo, e);
        }
    }

    private boolean isSim(String valor) {
        return valor != null && valor.trim().equalsIgnoreCase("Sim");
    }

    // ===============================================================
    // DADOS: TIPOS DE VIOLÊNCIA POR ANO (Lógica Inalterada)
    // ===============================================================
    public Map<String, Map<String, Integer>> dadosViolenciaPorAno() {
        Map<String, Map<String, Integer>> tabela = new TreeMap<>(); 
        for (String[] row : linhas) {
            String ano = row[1];
            tabela.putIfAbsent(ano, new TreeMap<>());
            Map<String, Integer> mapa = tabela.get(ano);

            if (isSim(row[11])) mapa.merge("Física", 1, Integer::sum);
            if (isSim(row[12])) mapa.merge("Psicológica", 1, Integer::sum);
            if (isSim(row[13])) mapa.merge("Tortura", 1, Integer::sum);
            if (isSim(row[14])) mapa.merge("Sexual", 1, Integer::sum);
        }
        return tabela;
    }

    // ===============================================================
    // DADOS: IDADE (Lógica Inalterada)
    // ===============================================================
    public Map<String, Integer> dadosCasosPorIdade() {
        Map<String, Integer> faixas = new LinkedHashMap<>();
        faixas.put("0-12", 0);
        faixas.put("13-17", 0);
        faixas.put("18-24", 0);
        faixas.put("25-35", 0);
        faixas.put("36-59", 0);
        faixas.put("60+", 0);

        for (String[] row : linhas) {
            try {
                double idade = Double.parseDouble(row[4]);
                if (idade <= 12) faixas.merge("0-12", 1, Integer::sum);
                else if (idade <= 17) faixas.merge("13-17", 1, Integer::sum);
                else if (idade <= 24) faixas.merge("18-24", 1, Integer::sum);
                else if (idade <= 35) faixas.merge("25-35", 1, Integer::sum);
                else if (idade <= 59) faixas.merge("36-59", 1, Integer::sum);
                else faixas.merge("60+", 1, Integer::sum);
            } catch (NumberFormatException e) {
                // Ignorar
            }
        }
        return faixas;
    }

    // ===============================================================
    // DADOS: CASOS POR HORA (Lógica Inalterada)
    // ===============================================================
    public Map<Integer, Integer> dadosCasosPorHora() {
        Map<Integer, Integer> contagem = new TreeMap<>();
        for (int i = 0; i < 24; i++) contagem.put(i, 0);

        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");

        for (String[] row : linhas) {
            String data = row[2];
            int hora = 12; 

            try {
                if (data != null && data.length() > 10) {
                    LocalDateTime dt = LocalDateTime.parse(data, formatter);
                    hora = dt.getHour();
                }
            } catch (Exception e) {
                // mantém hora padrão (12)
            }

            contagem.merge(hora, 1, Integer::sum);
        }
        return contagem;
    }

    // ===============================================================
    // MÉTODOS DE TABELA/DADOS BRUTOS (Inalterados)
    // ===============================================================
    
    public int totalRegistros() {
        return linhas.size();
    }

    public Map<String, Integer> casosPorMunicipio() {
        Map<String, Integer> mapa = new TreeMap<>();
        for (String[] row : linhas) {
            String municipio = row[9]; 
            mapa.merge(municipio, 1, Integer::sum);
        }
        return mapa;
    }

    public Map<String, Integer> violenciaFisicaPorMunicipio() {
        Map<String, Integer> mapa = new TreeMap<>();
        for (String[] row : linhas) {
            String municipio = row[9];
            String fisica = row[11]; 
            if (isSim(fisica)) {
                mapa.merge(municipio, 1, Integer::sum);
            }
        }
        return mapa;
    }
}