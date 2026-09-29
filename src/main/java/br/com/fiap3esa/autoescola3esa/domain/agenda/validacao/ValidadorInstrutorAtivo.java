package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

@Component
@RequiredArgsConstructor
public class ValidadorInstrutorAtivo implements ValidadorAgendamento {
    private final InstrutorRepository instrutorRepository;

    @Override
    public void validar(DadosAgendamentoInstrucao dados) {
        if (dados.idInstrutor() == null) {
            return;
        }
        if (!instrutorRepository.existsByIdAndAtivoTrue(dados.idInstrutor())) {
            throw new ValidacaoException("Instrução não pode ser agendada com instrutor inativo!");
        }
    }
}