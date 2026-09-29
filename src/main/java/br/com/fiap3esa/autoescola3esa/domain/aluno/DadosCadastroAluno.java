package br.com.fiap3esa.autoescola3esa.domain.aluno;

import br.com.fiap3esa.autoescola3esa.domain.endereco.DadosEndereco;
import jakarta.validation.Valid;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.Pattern;

public record DadosCadastroAluno(
        @NotBlank
        String nome,

        @NotBlank
        @Email
        String email,

        @NotBlank
        String telefone,

        @NotBlank
        @Pattern(regexp = "[0-9]{11}")
        String cpf,

        @Valid
        DadosEndereco endereco) {
}
