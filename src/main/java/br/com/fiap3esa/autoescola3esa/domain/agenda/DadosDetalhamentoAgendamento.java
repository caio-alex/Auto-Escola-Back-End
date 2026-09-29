package br.com.fiap3esa.autoescola3esa.domain.agenda;

import br.com.fiap3esa.autoescola3esa.domain.instrutor.Especialidade;

import java.time.LocalDateTime;

public record DadosDetalhamentoAgendamento(
        Long id,
        String nomeAluno,
        String nomeInstrutor,
        Especialidade especialidade,
        LocalDateTime dataHora,
        boolean cancelada) {
    public DadosDetalhamentoAgendamento(Instrucao instrucao) {
        this(
                instrucao.getId(),
                instrucao.getAluno().getNome(),
                instrucao.getInstrutor().getNome(),
                instrucao.getInstrutor().getEspecialidade(),
                instrucao.getDataHora(),
                instrucao.isCancelada()
        );
    }
}
