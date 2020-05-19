pipeline {
    agent { docker { image 'mcr.microsoft.com/dotnet/core/sdk:latest' } }
    environment {
        HOME = '/tmp'
    } 
    stages {
        stage ('Install dependencies')
        {
            steps
            {
                sh 'dotnet restore'
            }
        }
        stage ('Build')
        {
            steps
            {
                sh 'dotnet publish ./Api.Hosting/Api.Hosting.csproj -c Release -o ${OUTPUT_PATH}'
            }
        }
    }
    post
    {
        success
        {
            zip zipFile: 'api-package.zip', archive: true, dir: OUTPUT_PATH
            sh 'rm -rf api-package.zip'
        }
    }
}
