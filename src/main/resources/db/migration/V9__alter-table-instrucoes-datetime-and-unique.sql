alter table instrucoes modify column data_hora datetime not null;

alter table instrucoes
    add constraint uk_instrucoes_instrutor_data_hora unique (instrutor_id, data_hora);
