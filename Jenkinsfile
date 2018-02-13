pipeline {
  agent any
  stages {
    stage('testing stage') {
      steps {
        cleanWs()
      }
    }
    stage('deploying') {
      steps {
        cleanWs()
      }
    }
    stage('Success') {
      steps {
        sleep 10
      }
    }
  }
  environment {
    Test = 'test'
  }
}