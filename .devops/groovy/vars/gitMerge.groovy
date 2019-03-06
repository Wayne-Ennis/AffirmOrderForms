def call(body){
    def config = [:]
        body.resolveStrategy = Closure.DELEGATE_FIRST
        body.delegate = config
        body()

        def baseBranch = config.baseBranch
        def newBranch = config.newBranch

        echo "Going back to master branch"
        powershell(returnStatus: true, script: "git checkout ${baseBranch} 2> \$null")

        echo "merging ${newBranch} into ${baseBranch}"
        powershell(returnStatus: true, script: "git merge --no-ff -m \"Jenkins Merging Integration Build: ${newBranch} \" ${newBranch} 2> \$null")

        echo "Pushing Code Changes to Github."
       // powershell(returnStatus: true, script: "git push origin master 2> \$null")
}