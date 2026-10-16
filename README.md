# MyHome

MyHome runs home automation alongside a Blazor energy dashboard. The dashboard displays Tibber's quarter-hour spot prices (colored by MyHome's internal price level), reported hourly consumption aligned below the price chart, the cheapest upcoming interval, daily consumption and cost, and a read-only outlook for the heat pump, both bath thermostats, garage thermostat and basement dehumidifier. Hourly consumption is only plotted where Tibber has reported a reading; unpublished hours are not shown as zero. No Tibber credentials are sent to the browser.

`GET /automation/preview` returns the current and upcoming automation targets, their `EnergyPriceDetails`, profile baselines and the status of the current-hour consumption override. The current target includes the Home Assistant override when available; future targets are estimates based on published prices, the device profile and time rules, without a future consumption override. The preview never writes to devices and is not a report of their actual state.

Run `dotnet run --project MyHome.AppHost` to start the API and the `myhome-web` dashboard together with Aspire. Configure the existing Tibber, Home Assistant, and thermostat credentials using the AppHost user secrets or environment variables. The dashboard is available at the `myhome-web` URL shown by Aspire.

For separate local development, run `dotnet run --project MyHome.ApiService --launch-profile http` and `dotnet run --project MyHome.Web --launch-profile http`, then visit `http://localhost:5108`. Without Aspire, the development dashboard connects to the API at `http://localhost:5599`. For other deployments, set `ApiService__BaseUrl` to the API address; when unset, the dashboard uses Aspire service discovery for `myhome-api`.
