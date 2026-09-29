package br.com.fiap3esa.autoescola3esa.domain.aluno;

import br.com.fiap3esa.autoescola3esa.domain.endereco.DadosEndereco;
import jakarta.validation.constraints.NotNull;

public record DadosAtualizacaoAluno(
        @NotNull
        Long id,
        String nome,
        String email,
        String telefone,
        DadosEndereco endereco) {
}
