
create or alter procedure adauga_Carte
				@titlu varchar(50), @domeniu varchar(50)
as
begin
	begin tran
		begin try
			
			if dbo.validare_text(@titlu) <> 1
			begin
				print 'Titlu'
				raiserror('Titlu invalid', 14, 1);
			end

			declare @id_domeniu int = dbo.verific_existenta_domeniu(@domeniu);
						if @id_domeniu = 0
			begin
				print 'Domeniu'
				raiserror('Domeniu inexistent', 14, 1);
			end

			insert into Carte(titlu, id_domeniu) values (@titlu, @id_domeniu);
			print 'Carte adaugata'

			commit tran
			print 'Transaction committed'
		end try

		begin catch
			rollback tran
			print ERROR_MESSAGE();
			print 'Transation rollbacked'
			return 0
		end catch

		return 1
end

go
create or alter procedure adauga_Librarie
			@nume varchar(50), @adresa varchar(50)
as
begin 
	begin tran
		begin try

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

			insert into Librarie(nume, adresa) values (@nume, @adresa);
			print 'Librarie adaugata'

			commit tran
			print 'Transaction committed'

		end try

		begin catch
			rollback tran
			print ERROR_MESSAGE();
			print 'Transaction rollbacked'
			return 0
		end catch

		return 1
end

go
create or alter procedure adauga_CarteLibrarie2
				@titlu varchar(50), @domeniu varchar(50), 
				@nume varchar(50), @adresa varchar(50),
				@data_cumparare date
as
begin

	declare @carte_adaugata int;
	declare @librarie_adaugata int;

	execute @carte_adaugata = adauga_Carte @titlu, @domeniu
	execute @librarie_adaugata = adauga_Librarie @nume, @adresa

	if @carte_adaugata <> 1
	begin
		print 'Cartea nu a fost adaugat, deci nu putem adauga in CarteLibrarie'
		return 0
	end

	if @librarie_adaugata <> 1
	begin
		print 'Libraria nu a fost adaugata, deci nu putem adauga in CarteLibrarie'
		return 0
	end

	declare @id_carte int;
	declare @id_librarie int;

	select top 1 @id_carte = id_carte from Carte where titlu = @titlu;
	select top 1 @id_librarie = id_librarie from Librarie where nume = @nume;

	insert into CarteLibrarie(id_carte, id_librarie, data_cumparare) 
	values (@id_carte, @id_librarie, @data_cumparare);
	print 'Am adaugat o inregistrare noua in CarteLibrarie'
end;

select * from Domeniu;
select * from Carte;
select * from Librarie;
select * from CarteLibrarie;

execute adauga_CarteLibrarie2 'titluTEST1', 'domeniu1', 'numeTEST1', 'adresaTEST1', '2025-10-10'; -- ruleaza ok

execute adauga_CarteLibrarie2 'titluTEST2', '', 'numeTEST2', 'adresaTEST2', '2025-10-10'; -- commit la librarie dupa rollback

execute adauga_CarteLibrarie2 'titluTEST2', 'domeniu4', '', 'adresaTEST2', '2025-10-10'; -- commit la carte dupa rollback

execute adauga_CarteLibrarie2 'titluTEST3', '', '', 'adresaTEST3', '2025-10-10'; -- rollback