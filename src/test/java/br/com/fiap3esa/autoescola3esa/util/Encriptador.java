package br.com.fiap3esa.autoescola3esa.util;

import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;

public class Encriptador {
    public static void main(String[] args) {
        BCryptPasswordEncoder encoder = new BCryptPasswordEncoder();
        String hashSenha = encoder.encode("admin");
        System.out.println(hashSenha);
    }
}