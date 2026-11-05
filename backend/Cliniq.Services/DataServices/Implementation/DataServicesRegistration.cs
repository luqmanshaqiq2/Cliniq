using Cliniq.DataAccess.Repos;
using Cliniq.DataAccess.Repos.interfaces;
using Cliniq.Services.DataServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cliniq.Services.DataServices.Implementation;

public static class DataServicesRegistration
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentRepo, AppointmentRepo>();
        services.AddScoped<IAvailabilitySlotRepo, AvailabilitySlotRepo>();
        services.AddScoped<IDoctorRepo, DoctorRepo>();
        services.AddScoped<IInvoiceRepo, InvoiceRepo>();
        services.AddScoped<IMedicalRecordRepo, MedicalRecordRepo>();
        services.AddScoped<IPatientRepo, PatientRepo>();
        services.AddScoped<IPaymentRepo, PaymentRepo>();
        services.AddScoped<IUserRepo, UserRepo>();

        services.AddScoped<IAppointmentDataService, AppointmentDataService>();
        services.AddScoped<IAvailabilitySlotDataService, AvailabilitySlotDataService>();
        services.AddScoped<IDoctorDataService, DoctorDataService>();
        services.AddScoped<IInvoiceDataService, InvoiceDataService>();
        services.AddScoped<IMedicalRecordDataService, MedicalRecordDataService>();
        services.AddScoped<IPatientDataService, PatientDataService>();
        services.AddScoped<IPaymentDataService, PaymentDataService>();
        services.AddScoped<IUserDataService, UserDataService>();
        return services;
    }
}
