# $language = "python"
# $interface = "1.0"

import re
import os

promptString = "ok."
promptTimeout = 1
includePattern = "^include\s+([\.\w/-]+)"
commentPattern = "^\\\s*"

def sendFile(file, level=0):
    print("Sending file " + file)

    if level>10:
        crt.Dialog.MessageBox("Too many nested include files")
        raise "Too many include levels"

    with open(file, "r") as f:
        for line in f:
            inc = re.match(includePattern, line)
            if inc:
                incfile = inc.group(1)
                sendFile(os.path.join(os.path.dirname(file), incfile), level + 1)
                continue

            if re.match(commentPattern, line):
                continue

            crt.Screen.Send(line)
            crt.Screen.WaitForString(promptString, promptTimeout)

def main():
    crt.Screen.Synchronous = True
    filePath = crt.Dialog.FileOpenDialog(title="Select forth file to upload")
    if filePath:
        sendFile(filePath, level=0)
    crt.Screen.Synchronous = False
main()
