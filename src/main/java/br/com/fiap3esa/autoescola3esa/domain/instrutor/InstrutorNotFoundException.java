package br.com.fiap3esa.autoescola3esa.domain.instrutor;

public class InstrutorNotFoundException extends RuntimeException {
    public InstrutorNotFoundException(String message) {
        super(message);
    }
}