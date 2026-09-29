package br.com.fiap3esa.autoescola3esa.domain.agenda.validacao;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.ValidacaoException;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;

@Component
public class ValidadorHoraInteira implements ValidadorAgendamento {
    @Override
    public void validar(DadosAgendamentoInstrucao dados) {
        LocalDateTime dataHoraEscolhida = dados.dataHora();

        if (dataHoraEscolhida.getMinute() != 0) {
            throw new ValidacaoException("O horário deve ser preenchido com horas inteiras (ex: 09:00, 13:00, ...");
        }
    }
}