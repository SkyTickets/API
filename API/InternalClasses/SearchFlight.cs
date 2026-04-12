using API.Model;

using Microsoft.EntityFrameworkCore;

namespace API.InternalClasses
{
    internal static class SearchFlight
    {
        public static async Task<List<Flight>> FindFLightsAsync(PostgresContext context, string from, string to, DateOnly start, DateOnly end, int min = -1, int max = -1, string? airline = null)
        {
            Airport? airportFrom = await context.Airports.FirstOrDefaultAsync(x => x.ApCity.ToLower() == from.ToLower());

            if (airportFrom is null)
            {
                return [];
            }

            Airport? airportTo = await context.Airports.FirstOrDefaultAsync(x => x.ApCity.ToLower() == to.ToLower());
            
            if (airportTo is null)
            {
                return [];
            }

            List<Flight> flights = await context.Flights
                .Where(x => x.FDepartureAirport == airportFrom.ApId)
                .Where(x => x.FArrivalAirport == airportTo.ApId)
                .Where(x => new DateOnly(x.FDepartureTime.Year, x.FDepartureTime.Month, x.FDepartureTime.Day) >= start)
                .Where(x => new DateOnly(x.FArrivalTime.Year, x.FArrivalTime.Month, x.FArrivalTime.Day) <= end)
                .ToListAsync();

            if (min != -1)
            {
                flights = [.. flights.Where(x => x.FPrice >= min)];
            }

            if (max != -1)
            {
                flights = [.. flights.Where(x => x.FPrice <= max)];
            }

            if (airline is not null && airline != string.Empty)
            {
                Airline? gottenAirline = await context.Airlines.FirstOrDefaultAsync(x => x.AlName.ToLower() == airline.ToLower());

                if (gottenAirline is null)
                {
                    return [];
                }

                flights = [.. flights.Where(x => x.FAirline <= gottenAirline.AlId)];
            }

            return flights;
        }
    }
}
