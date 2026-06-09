package com.api.dashboard.Repository;

import com.api.dashboard.Dominio.RegistroViolencia;
import org.springframework.stereotype.Repository;

import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

@Repository
public class CsvRepository {

    private static final String CAMINHO = "main\\resources\\dados.csv";

    public List<RegistroViolencia> lerCsv() {
        List<RegistroViolencia> lista = new ArrayList<>();

        try {
            List<String> linhas = Files.readAllLines(Paths.get(CAMINHO));

            for (int i = 1; i < linhas.size(); i++) {
                String[] col = linhas.get(i).split(",");

                RegistroViolencia r = new RegistroViolencia();

                r.setUf(col[0]);
                r.setAno(Integer.parseInt(col[1]));
                r.setDtOcorrencia(col[2]);
                r.setDtNascimento(col[3]);
                r.setIdade(Double.parseDouble(col[4]));
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

                lista.add(r);
            }

        } catch (Exception e) {
            e.printStackTrace();
        }

        return lista;
    }
}