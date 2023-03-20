newman run "https://api.getpostman.com/collections/2c49b8c3-0f1a-43f5-8a18-5d7f6c50c0ac?apikey=${env:POSTMAN_API_KEY}" --reporters "cli,junit" --reporter-junit-export TestResults\result2.xml
