file1 = open('newPoliCheckIssue', 'r') 
lines = file1.readlines()
outlist = []
totalCount = 0

fileName =""
for line in lines:
    tagObject = line[1:7]
    if tagObject == "Object":
        fileName = line.split(" ")[2].split("\"")[1]
        #print(fileName)
    tagOccurences = line[1:11]
    if tagOccurences == "Occurences":
        count = line[12:-14]
        if count != "N/A":
            count = int(count)
            if count > 0:
                totalCount += count
                outline = "- " + "\"" + fileName[10:] + "\""
                #print(outline)
                outlist.append(outline)
print(totalCount)
with open("filesNamesToSkipNew.txt", 'a') as f:
    for outline in outlist:
        f.write(outline+"\n")
        
    
    