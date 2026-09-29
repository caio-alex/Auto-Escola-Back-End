package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.InstrucaoRepository;
import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class ValidadorDisponibilidadeInstrutor implements ValidadorAgendamento {
    private final InstrucaoRepository repository;

    @Override
    public void validar(DadosAgendamentoInstrucao dados) {
        boolean instrutorOcupado = repository.existsByInstrutorIdAndDataHoraAndCanceladaFalse(
                dados.idInstrutor(),
                dados.dataHora()
        );

        if (instrutorOcupado) {
            throw new ValidacaoException("Instrutor ocupado na data/hora escolhida!");
        }
    }
}