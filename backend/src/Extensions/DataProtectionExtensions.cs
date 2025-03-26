using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
namespace PizzaAI.Extensions;

public static class DataProtectionExtensions
{
    public static IDataProtectionBuilder AddSecureDataProtection(
        this IServiceCollection services,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        // In development, use the default data protection configuration
        if (env.IsDevelopment())
        {
            return services.AddDataProtection()
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }

        try
        {
            // Load the certificate from the file
            var certPath = Environment.GetEnvironmentVariable("CERT_PATH");
            var certPassword = config["CERT_PASSWORD"];
            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                Path.Combine(certPath!, "certificate.pfx"),
                certPassword!,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet
            );

            // Configure the data protection with the certificate
            return services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
                .ProtectKeysWithCertificate(certificate)
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during certificate configuration : {e.Message}");

            // Set the default data protection configuration without certificate
            return services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
                .SetApplicationName("PizzaAI")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
        }
    }
}
