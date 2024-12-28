namespace TDNS.DNSStorage;

enum DNSType : UInt16
{
    A = 1, NS = 2, CNAME = 5, SOA = 6, PTR = 12, MX = 15, TXT = 16, AAAA = 28, SRV = 33, NAPTR = 35, DS = 43, RRSIG = 46,
    NSEC = 47, DNSKEY = 48, NSEC3 = 50, OPT = 41, IXFR = 251, AXFR = 252, ANY = 255, CAA = 257
}

enum DNSClass : UInt16 { IN = 1, CH = 3 }

public enum DNSRecordPlace { ANSWER, AUTHORITY, ADDITIONAL }
