using System.Data;
using Microsoft.Data.SqlClient;

namespace EczaneApp
{
    // Veritabanı bağlantısını açar ve parametreli komutları çalıştırır.
    public class Veritabani
    {
        // SSMS'e bağlanırken "Server name" kutusunda ne yazıyorsa Server= kısmına onu yaz:
        //   localhost             -> Server=localhost;
        //   .\SQLEXPRESS          -> Server=.\SQLEXPRESS;
        //   (localdb)\MSSQLLocalDB -> Server=(localdb)\MSSQLLocalDB;
        private readonly string baglantiCumlesi =
            @"Server=.\SQLEXPRESS;Database=EczaneDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection Ac()
        {
            var baglanti = new SqlConnection(baglantiCumlesi);
            try
            {
                baglanti.Open();
                return baglanti;
            }
            catch
            {
                baglanti.Dispose();
                throw;
            }
        }

        // SELECT sorguları için: sonucu tablo olarak döndürür
        public DataTable Sorgula(string sql, params SqlParameter[] parametreler)
        {
            using var baglanti = Ac();
            using var komut = new SqlCommand(sql, baglanti);
            komut.Parameters.AddRange(parametreler);
            var tablo = new DataTable();
            tablo.Load(komut.ExecuteReader());
            return tablo;
        }

        // INSERT / UPDATE / DELETE için: etkilenen satır sayısını döndürür
        public int Calistir(string sql, params SqlParameter[] parametreler)
        {
            using var baglanti = Ac();
            using var komut = new SqlCommand(sql, baglanti);
            komut.Parameters.AddRange(parametreler);
            return komut.ExecuteNonQuery();
        }

        // COUNT, SUM gibi tek değer dönen sorgular için
        public object TekDeger(string sql, params SqlParameter[] parametreler)
        {
            using var baglanti = Ac();
            using var komut = new SqlCommand(sql, baglanti);
            komut.Parameters.AddRange(parametreler);
            return komut.ExecuteScalar();
        }
    }
}
