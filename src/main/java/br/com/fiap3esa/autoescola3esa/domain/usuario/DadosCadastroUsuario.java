package br.com.fiap3esa.autoescola3esa.domain.usuario;

import jakarta.validation.constraints.NotBlank;

public record DadosCadastroUsuario(
        @NotBlank
        String login,

        @NotBlank
        String senha,

        Perfil perfil) {
}
