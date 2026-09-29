package br.com.fiap3esa.autoescola3esa.domain.instrutor;

import br.com.fiap3esa.autoescola3esa.domain.agenda.Instrucao;
import br.com.fiap3esa.autoescola3esa.domain.aluno.Aluno;
import br.com.fiap3esa.autoescola3esa.domain.endereco.Endereco;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.data.jpa.test.autoconfigure.DataJpaTest;
import org.springframework.boot.jdbc.test.autoconfigure.AutoConfigureTestDatabase;
import org.springframework.boot.jpa.test.autoconfigure.TestEntityManager;
import org.springframework.test.context.ActiveProfiles;

import java.time.DayOfWeek;
import java.time.LocalDateTime;
import java.time.temporal.TemporalAdjusters;

import static org.assertj.core.api.Assertions.assertThat;

@DataJpaTest
@AutoConfigureTestDatabase(replace = AutoConfigureTestDatabase.Replace.NONE)
@ActiveProfiles("test")
public class InstrutorRepositoryTest {
    @Autowired
    InstrutorRepository repository;

    @Autowired
    TestEntityManager testEntity;

    @Test
    @DisplayName("Expecativa: retornar null quando instrutor cadastrado estiver ocupado.")
    void escolherInstrutorAleatorioDisponivelCenario1() {
        LocalDateTime proximaSegundaAs10 = LocalDateTime
                .now()
                .with(TemporalAdjusters.next(DayOfWeek.MONDAY))
                .withHour(10)
                .withMinute(0)
                .withSecond(0)
                .withNano(0);

        //cadastrar aluno
        Aluno aluno = cadastrarAluno();

        //cadastrar instrutor
        Instrutor instrutor = cadastrarInstrutor();

        //agendar instrução
        agendarInstrucao(aluno, instrutor, proximaSegundaAs10);

        //act or when
        Instrutor instrutorDisponivel = repository.escolherInstrutorAleatorioDisponivel(
                Especialidade.MOTOS,
                proximaSegundaAs10
        );

        //assert or then
        assertThat(instrutorDisponivel).isNull();
    }

    @Test
    @DisplayName("Expecativa: retornar instrutor quando instrutor cadastrado estiver disponível.")
    void escolherInstrutorAleatorioDisponivelCenario2() {
        LocalDateTime proximaSegundaAs10 = LocalDateTime
                .now()
                .with(TemporalAdjusters.next(DayOfWeek.MONDAY))
                .withHour(10)
                .withMinute(0)
                .withSecond(0)
                .withNano(0);

        //cadastrar instrutor
        Instrutor instrutor = cadastrarInstrutor();

        //act or when
        Instrutor instrutorDisponivel = repository.escolherInstrutorAleatorioDisponivel(
                Especialidade.MOTOS,
                proximaSegundaAs10
        );

        //assert or then
        assertThat(instrutorDisponivel).isEqualTo(instrutor);
    }

    private Endereco dadosEndereco() {
        return new Endereco(
                "Rua Teste",
                "000",
                "Casa dos Fundos",
                "Vila Teste",
                "Test City",
                "TS",
                "01234-567"
        );
    }

    private Aluno cadastrarAluno() {
        Aluno aluno = new Aluno(
                null,
                "Aluno Teste",
                "alunoteste@email.com",
                "(11) 98765-4321",
                "12345678901",
                dadosEndereco(),
                true
        );
        testEntity.persist(aluno);
        return aluno;
    }

    private Instrutor cadastrarInstrutor() {
        Instrutor instrutor = new Instrutor(
                null,
                "Instrutor Teste",
                "instrutorteste@email.com.br",
                "(11) 91234-5678",
                "01234567890",
                Especialidade.MOTOS,
                dadosEndereco(),
                true
        );
        testEntity.persist(instrutor);
        return instrutor;
    }

    private void agendarInstrucao(
            Aluno aluno,
            Instrutor instrutor,
            LocalDateTime dataHora) {
        Instrucao instrucao = new Instrucao(
                null,
                aluno,
                instrutor,
                dataHora
        );
        testEntity.persist(instrucao);
    }
}