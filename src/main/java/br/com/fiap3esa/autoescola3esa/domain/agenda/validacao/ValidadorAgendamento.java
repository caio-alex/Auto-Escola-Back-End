package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;

public interface ValidadorAgendamento {
    void validar(DadosAgendamentoInstrucao dados);
}