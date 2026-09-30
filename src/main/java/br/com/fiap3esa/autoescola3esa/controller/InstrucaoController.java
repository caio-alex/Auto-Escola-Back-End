package br.com.fiap3esa.autoescola3esa.controller;

import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosAgendamentoInstrucao;
import br.com.fiap3esa.autoescola3esa.domain.agenda.DadosDetalhamentoAgendamento;
import br.com.fiap3esa.autoescola3esa.domain.usuario.Usuario;
import br.com.fiap3esa.autoescola3esa.service.AgendaDeInstrucoes;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springdoc.core.annotations.ParameterObject;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.data.web.PageableDefault;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.util.UriComponentsBuilder;

import java.net.URI;

@RestController
@RequestMapping("/instrucoes")
@RequiredArgsConstructor
@SecurityRequirement(name = "bearer-key")
public class InstrucaoController {
    private final AgendaDeInstrucoes agenda;

    @PostMapping
    @PreAuthorize("hasAnyRole('ADMIN', 'USER', 'ALUNO')")
    public ResponseEntity<DadosDetalhamentoAgendamento> agendarInstrucao(
            @RequestBody @Valid DadosAgendamentoInstrucao dados,
            @AuthenticationPrincipal Usuario usuario,
            UriComponentsBuilder uriBuilder) {
        DadosDetalhamentoAgendamento dto = agenda.agendar(dados, usuario);
        URI uri = uriBuilder
                .path("/instrucoes/{id}")
                .buildAndExpand(dto.id())
                .toUri();
        return ResponseEntity.created(uri).body(dto);
    }

    @GetMapping
    @PreAuthorize("hasAnyRole('ADMIN', 'USER', 'ALUNO', 'INSTRUTOR')")
    public ResponseEntity<Page<DadosDetalhamentoAgendamento>> listarInstrucoes(
            @ParameterObject @PageableDefault(size = 10, sort = "dataHora") Pageable paginacao,
            @AuthenticationPrincipal Usuario usuario) {
        return ResponseEntity.ok(agenda.listar(paginacao, usuario));
    }

    @GetMapping("/{id}")
    @PreAuthorize("hasAnyRole('ADMIN', 'USER', 'ALUNO', 'INSTRUTOR')")
    public ResponseEntity<DadosDetalhamentoAgendamento> detalharInstrucao(
            @PathVariable Long id,
            @AuthenticationPrincipal Usuario usuario) {
        return ResponseEntity.ok(agenda.detalhar(id, usuario));
    }

    @DeleteMapping("/{id}")
    @PreAuthorize("hasAnyRole('ADMIN', 'USER', 'ALUNO', 'INSTRUTOR')")
    public ResponseEntity<Void> cancelarInstrucao(
            @PathVariable Long id,
            @AuthenticationPrincipal Usuario usuario) {
        agenda.cancelar(id, usuario);
        return ResponseEntity.noContent().build();
    }
}
