package br.com.fiap3esa.autoescola3esa.domain.instrutor;

import br.com.fiap3esa.autoescola3esa.domain.endereco.DadosEndereco;
import jakarta.validation.constraints.NotNull;

public record DadosAtualizacaoInstrutor(
        @NotNull
        Long id,
        String nome,
        String email,
        String telefone,
        Especialidade especialidade,
        DadosEndereco endereco) {
}