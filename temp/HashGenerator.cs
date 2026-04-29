using BCrypt.Net;

string password = "Test1234";
string hash = BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine($"Hash: {hash}");
