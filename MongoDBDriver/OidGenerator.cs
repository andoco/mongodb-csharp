﻿
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MongoDB.Driver
{
    public class OidGenerator
    {
        internal static DateTime epoch = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
        
        private volatile int inc;
        
        private byte[] machineHash;
        private byte[] procID;
        
        public OidGenerator(){
            GenerateConstants();
        }
        
        public Oid Generate(){
            //FIXME Endian issues with this code.  
            //.Net runs in native endian mode which is usually little endian.  
            //Big endian machines don't need the reversing (Linux+PPC, XNA on XBox)
            byte[] oid = new byte[12];
            int copyidx = 0;

            byte[] time = BitConverter.GetBytes(GenerateTime());
            Array.Reverse(time);
            Array.Copy(time,0,oid,copyidx,4);
            copyidx += 4;

            Array.Copy(machineHash,0,oid,copyidx,3);
            copyidx += 3;
                       
            Array.Copy(this.procID,2,oid,copyidx,2);
            copyidx += 2;
            
            byte[] inc = BitConverter.GetBytes(GenerateInc());
            Array.Reverse(inc);
            Array.Copy(inc,1,oid,copyidx,3);
            
            return new Oid(oid);            
        }
        
        private int GenerateTime(){
            DateTime now = DateTime.UtcNow;
            //DateTime nowtime = new DateTime(epoch.Year, epoch.Month, epoch.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
            TimeSpan diff = now - epoch;
            return Convert.ToInt32(Math.Floor(diff.TotalSeconds));            
        }
        
        private int GenerateInc(){
        	return Interlocked.Increment(ref inc);
        }
        
        private void GenerateConstants(){
            this.machineHash = GenerateHostHash();
            this.procID = BitConverter.GetBytes(GenerateProcId());
            Array.Reverse(this.procID);
        }
        
        private byte[] GenerateHostHash(){            
            MD5 md5 = MD5.Create();            
            string host = System.Net.Dns.GetHostName();
            return md5.ComputeHash(Encoding.Default.GetBytes(host));            
        }
        
        private int GenerateProcId(){
            Process proc = Process.GetCurrentProcess();
            return proc.Id;            
        }

    }
}
