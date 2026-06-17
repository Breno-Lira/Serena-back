package com.api.dashboard.Repository;

import com.api.dashboard.Dominio.RegistroViolencia;
import com.opencsv.CSVReader;
import org.springframework.core.io.ClassPathResource;
import org.springframework.stereotype.Repository;

import java.io.InputStreamReader;
import java.io.Reader;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

@Repository
public class CsvRepository {

    private static final String ARQUIVO = "dados.csv";

    public List<RegistroViolencia> lerCsv() {
        List<RegistroViolencia> lista = new ArrayList<>();

        try (Reader reader = new InputStreamReader(
                new ClassPathResource(ARQUIVO).getInputStream(), StandardCharsets.UTF_8);
             CSVReader csv = new CSVReader(reader)) {

            String[] col;
            boolean primeira = true;

            while ((col = csv.readNext()) != null) {
                if (primeira) {            // pula o cabecalho
                    primeira = false;
                    continue;
                }
                if (col.length < 34) {     // linha incompleta -> ignora
                    continue;
                }
                lista.add(mapear(col));
            }
        } catch (Exception e) {
            throw new RuntimeException("Erro ao carregar o CSV: " + ARQUIVO, e);
        }

        return lista;
    }

    private RegistroViolencia mapear(String[] col) {
        RegistroViolencia r = new RegistroViolencia();

        r.setUf(col[0]);
        r.setAno(parseInt(col[1]));
        r.setDtOcorrencia(col[2]);
        r.setDtNascimento(col[3]);
        r.setIdade(parseDouble(col[4]));
        r.setRaca(col[5]);
        r.setEscolaridade(col[6]);
        r.setLocalOcorrencia(col[7]);
        r.setGrupoIdade(col[8]);
        r.setMunicipio(col[9]);
        r.setMotivoViolencia(col[10]);

        r.setViolFisica(col[11]);
        r.setViolPsico(col[12]);
        r.setViolTort(col[13]);
        r.setViolSexual(col[14]);

        r.setSexAssedio(col[15]);
        r.setSexEstupro(col[16]);
        r.setSexExploracao(col[17]);
        r.setSexPornografia(col[18]);
        r.setSexOutro(col[19]);

        r.setRelPai(col[20]);
        r.setRelMae(col[21]);
        r.setRelPadrasto(col[22]);
        r.setRelMadrasta(col[23]);
        r.setRelConjugue(col[24]);
        r.setRelExConjugue(col[25]);
        r.setRelNamorado(col[26]);
        r.setRelExNamorado(col[27]);
        r.setRelFilho(col[28]);
        r.setRelIrmao(col[29]);
        r.setRelConhecido(col[30]);
        r.setRelDesconhecido(col[31]);

        r.setAutorSexo(col[32]);
        r.setOutVezes(col[33]);

        return r;
    }

    private Integer parseInt(String v) {
        try { return Integer.parseInt(v.trim()); } catch (Exception e) { return null; }
    }

    private Double parseDouble(String v) {
        try { return Double.parseDouble(v.trim()); } catch (Exception e) { return null; }
    }
}
