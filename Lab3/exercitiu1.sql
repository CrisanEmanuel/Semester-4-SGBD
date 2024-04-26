
create or alter function validare_text (@denumire varchar(50))
returns bit
as
begin
	declare @flag bit
	set @flag = 1
	if @denumire is null or @denumire = ''
		set @flag = 0;
	return @flag;
end
go

create or alter function verific_existenta_domeniu (@descriere varchar(50))
returns int
as
begin
	declare @id_domeniu int;
	select top 1 @id_domeniu = id_domeniu from Domeniu where descriere = @descriere;
	if @id_domeniu is null
		return 0;
	return @id_domeniu;
end
go

create or alter procedure adauga_CarteLibrarie
				@titlu varchar(50), @domeniu varchar(50), 
				@nume varchar(50), @adresa varchar(50),
				@data_cumparare date
as
begin
	begin tran
		begin try

			if dbo.validare_text(@titlu) <> 1
			begin
				print 'Titlu'
				raiserror('Titlu invalid', 14, 1);
			end

			if dbo.validare_text(@nume) <> 1
			begin
				print 'Nume'
				raiserror('Nume invalid', 14, 1);
			end

			if dbo.validare_text(@adresa) <> 1
			begin
				print 'Adresa'
				raiserror('Nume invalid', 14, 1);
			end

			declare @id_domeniu int = dbo.verific_existenta_domeniu(@domeniu);
			if @id_domeniu = 0
			begin
				print 'Domeniu'
				raiserror('Domeniu inexistent', 14, 1);
			end

			insert into Librarie(nume, adresa) values (@nume, @adresa);
			print 'Librarie adaugata'

			insert into Carte(titlu, id_domeniu) values (@titlu, @id_domeniu)
			print 'Carte adaugata'

			declare @id_carte int;
			declare @id_librarie int;

			select top 1 @id_carte = id_carte from Carte where titlu = @titlu;
			select top 1 @id_librarie = id_librarie from Librarie where nume = @nume;

			insert into CarteLibrarie(id_carte, id_librarie, data_cumparare) 
			values (@id_carte, @id_librarie, @data_cumparare);

			commit tran;
			print 'Transaction committed';
		end try

	begin catch
		rollback tran;
		print ERROR_MESSAGE();
		print 'Transaction rollbacked';
	end catch
end;

go
select * from Domeniu;
select * from Carte;
select * from Librarie;
select * from CarteLibrarie;

execute adauga_CarteLibrarie 'titlu8', 'domeniu1', 'nume6', 'adresa6', '2025-10-10'; -- pk deja existenta

execute adauga_CarteLibrarie 'titlu8', 'domeniu1000', 'nume6', 'adresa6', '2025-10-20'; -- domeniu inexsistent

execute adauga_CarteLibrarie 'titluTEST', 'domeniu2', 'nume5', 'adresa5', '2024-11-10'; -- ruleaza ok