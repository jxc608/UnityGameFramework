#include <sys/socket.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <err.h>

extern "C"
{
    const char * _GetVersionNumber()
    {
        NSString* number = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
        return strdup([number UTF8String]);
    }
    
    const char * _GetIPWithHostName(const char* name)
    {
        NSString * hostName = [NSString stringWithUTF8String:name];
        struct addrinfo * result;
        struct addrinfo * res;
        char ipv4[128];
        char ipv6[128];
        int error;
        BOOL IS_IPV6 = FALSE;
        bzero(&ipv4, sizeof(ipv4));
        bzero(&ipv4, sizeof(ipv6));
        
        error = getaddrinfo([hostName UTF8String], NULL, NULL, &result);
        if(error != 0) {
            NSLog(@"error in getaddrinfo:%d", error);
            return nil;
        }
        for(res = result; res!=NULL; res = res->ai_next) {
            char hostname[1025] = "";
            error = getnameinfo(res->ai_addr, res->ai_addrlen, hostname, 1025, NULL, 0, 0);
            if(error != 0) {
                NSLog(@"error in getnameifno: %s", gai_strerror(error));
                continue;
            }
            else {
                switch (res->ai_addr->sa_family) {
                    case AF_INET:
                        memcpy(ipv4, hostname, 128);
                        break;
                    case AF_INET6:
                        memcpy(ipv6, hostname, 128);
                        IS_IPV6 = TRUE;
                    default:
                        break;
                }
                NSLog(@"hostname: %s ", hostname);
            }
        }
        freeaddrinfo(result);
        if(IS_IPV6 == TRUE) return strdup(ipv6);
        return strdup(ipv4);
    }
    
}

