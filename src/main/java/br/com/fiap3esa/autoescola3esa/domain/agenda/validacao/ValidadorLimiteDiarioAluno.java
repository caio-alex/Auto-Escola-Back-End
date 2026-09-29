package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.InstrucaoRepository;
import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;

@Component
@RequiredArgsConstructor
public class ValidadorLimiteDiarioAluno implements ValidadorAgendamento {
    private final InstrucaoRepository repository;

    @Override
    public void validar(DadosAgendamentoInstrucao dados) {
        LocalDateTime inicio = dados.dataHora().withHour(6);
        LocalDateTime fim = dados.dataHora().withHour(21 - 1);
        boolean reicidencia = repository.existsByAlunoIdAndDataHoraBetweenAndCanceladaFalse(dados.idAluno(), inicio, fim);

        if (reicidencia) {
            throw new ValidacaoException("Permitido apenas um agendamento diário por aluno!");
        }
    }
}