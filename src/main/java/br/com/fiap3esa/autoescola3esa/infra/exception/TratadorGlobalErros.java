package br.com.fiap3esa.autoescola3esa.infra.exception;

import br.com.fiap3esa.autoescola3esa.domain.instrutor.InstrutorNotFoundException;
import jakarta.persistence.EntityNotFoundException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.List;

@RestControllerAdvice
public class TratadorGlobalErros {
    @ExceptionHandler(EntityNotFoundException.class)
    public ResponseEntity<Void> tratarEntityNotFound() {
        return ResponseEntity.notFound().build();
    }

    @ExceptionHandler(InstrutorNotFoundException.class)
    public ResponseEntity<DadosMessageNotFound> tratarInstrutorNotFound(InstrutorNotFoundException e) {
        return ResponseEntity.status(HttpStatus.NOT_FOUND).body(new DadosMessageNotFound(e.getMessage()));
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<List<DadosBadRequest>> tratarBadRequest(MethodArgumentNotValidException e) {
        List<FieldError> erros = e.getFieldErrors();
        return ResponseEntity.badRequest().body(erros.stream().map(DadosBadRequest::new).toList());
    }

    /*@ExceptionHandler(Exception.class)
    public ResponseEntity<DadosMessageNotFound> tratarGenericException(Exception e) {
        return ResponseEntity.unprocessableContent().body(new DadosMessageNotFound(e.getCause().getMessage()));
    }*/

    private record DadosBadRequest(String field, String message) {
        public DadosBadRequest(FieldError erro) {
            this(erro.getField(), erro.getDefaultMessage());
        }
    }

    private record DadosMessageNotFound(String message) {
    }
}