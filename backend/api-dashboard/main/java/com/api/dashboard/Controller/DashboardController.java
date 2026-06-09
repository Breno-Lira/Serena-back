package com.api.dashboard.Controller;

import com.api.dashboard.Service.DashboardService;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/dashboard")
public class DashboardController {

    private final DashboardService service;

    public DashboardController(DashboardService service) {
        this.service = service;
    }

    @GetMapping("/total")
    public int getTotal() {
        return service.totalRegistros();
    }

    @GetMapping("/municipios")
    public Object getMunicipios() {
        return service.casosPorMunicipio();
    }

    @GetMapping("/violencia-fisica")
    public Object getViolenciaFisica() {
        return service.violenciaFisicaPorMunicipio();
    }

    // Conteúdo a ser adicionado no DashboardController

    @GetMapping("/violencia-por-ano")
    public Object getViolenciaPorAno() {
        return service.dadosViolenciaPorAno();
    }

    @GetMapping("/casos-por-idade")
    public Object getCasosPorIdade() {
        return service.dadosCasosPorIdade();
    }

    @GetMapping("/casos-por-hora")
    public Object getCasosPorHora() {
        return service.dadosCasosPorHora();
    }
}



