alter table instrucoes add column cancelada tinyint not null default 0;

-- O índice único antigo também sustentava a foreign key de instrutor_id.
-- O MySQL não deixa remover o último índice que sustenta uma FK, então
-- criamos um índice simples antes de remover o antigo.
create index idx_instrucoes_instrutor_id on instrucoes (instrutor_id);

alter table instrucoes drop index uk_instrucoes_instrutor_data_hora;

-- Coluna virtual: fica NULL quando a instrução está cancelada. Como o MySQL
-- não trata múltiplos NULLs como duplicados em índices únicos, o mesmo
-- instrutor/horário pode ser reagendado depois de um cancelamento, mas
-- continua bloqueado enquanto a instrução ativa existir.
alter table instrucoes
    add column data_hora_ativa datetime
        generated always as (case when cancelada = 0 then data_hora else null end) virtual;

alter table instrucoes
    add constraint uk_instrucoes_instrutor_data_hora_ativa unique (instrutor_id, data_hora_ativa);
