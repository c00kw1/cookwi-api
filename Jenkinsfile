pipeline {
    parameters
    {
        booleanParam(name: 'PRODUCES_ARTEFACTS', defaultValue: params.PRODUCES_ARTEFACTS ?: false)
        string(name: 'OUTPUT_PATH', defaultValue: params.OUTPUT_PATH ?: './dist')
        string(name: 'ENVIRONMENT', defaultValue: params.ENVIRONMENT ?: 'homologation')
        string(name: 'SDK', defaultValue: params.SDK ?: 'latest')
    }
    agent
    {
        docker
        {
            image "mcr.microsoft.com/dotnet/sdk:${params.SDK}"
            args "--name ${env.BUILD_TAG}"
        }
    }
    environment {
        HOME = '/tmp'
    } 
    stages {
        stage ('Install dependencies')
        {
            steps
            {
                sh "dotnet restore"
            }
        }
        stage ('Build')
        {
            steps
            {
                sh "dotnet publish ./Api.Hosting/Api.Hosting.csproj -c Release -o ${params.OUTPUT_PATH}"
            }
        }
        stage ('Artifacts')
        {
            when { expression { params.PRODUCES_ARTEFACTS == true } } // if pull_request, we don't want artifacts
            steps
            {
                zip zipFile: 'api-package.zip', archive: true, dir: params.OUTPUT_PATH
                zip zipFile: 'docker-config.zip', archive: true, dir: "docker/"
            }
        }
    }
    post
    {
        always
        {
            deleteDir()
        }
    }
}
