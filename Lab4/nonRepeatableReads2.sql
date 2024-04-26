
-- NON-REPEATABLE READS
begin transaction
	select * from Librarie where adresa = 'OLD';
	waitfor delay '00:00:10'
	select * from Librarie where adresa = 'OLD';
commit transaction

set transaction isolation level repeatable read -- PROBLEM: READ COMMITTED
begin transaction
	select * from Librarie where adresa = 'OLD';
	waitfor delay '00:00:10'
	select * from Librarie where adresa = 'OLD';
commit transaction