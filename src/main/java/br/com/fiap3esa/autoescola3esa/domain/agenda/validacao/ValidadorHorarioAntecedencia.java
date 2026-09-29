package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import org.springframework.stereotype.Component;

import java.time.Duration;
import java.time.LocalDateTime;

@Component
public class ValidadorHorarioAntecedencia implements ValidadorAgendamento {
    @Override
    public void validar(DadosAgendamentoInstrucao dados) {
        LocalDateTime dataHoraEscolhida = dados.dataHora();
        LocalDateTime agora = LocalDateTime.now();

        long antedencia = Duration.between(agora, dataHoraEscolhida).toMinutes();

        if (antedencia < 30) {
            throw new ValidacaoException("Necessária antecedência mínima de 30 min. para agendamento de instrução!");
        }
    }
}