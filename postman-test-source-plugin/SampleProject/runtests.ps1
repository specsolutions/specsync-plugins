newman run "https://api.getpostman.com/collections/26495037-5c8d6f15-b2a3-4b47-ae1d-72dd3bc1b49b?apikey=${env:POSTMAN_API_KEY}" --reporters "cli,junit" --reporter-junit-export TestResults\result.xml
