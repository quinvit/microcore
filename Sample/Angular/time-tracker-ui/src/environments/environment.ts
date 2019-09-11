// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  config: {
    tenant: '2bac2706-6fdc-46af-93aa-02f86134156d',
    clientId: '14b6b257-2e56-4994-9c40-51e2cc03fa19',
    endpoints: {
      'https://graph.microsoft.com': '00000003-0000-0000-c000-000000000000',
      'http://localhost:5000': '14b6b257-2e56-4994-9c40-51e2cc03fa19'
    },
    apiGateway: 'http://localhost:5000'
  }
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
