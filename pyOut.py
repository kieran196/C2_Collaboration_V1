import serial
import time, datetime
import pdb

# HAD TO REMOVE MOST COMMENTS DUE TO NON-ASCII CHARACTERS (FRENCH)

# Version PYTHON: 3.5.2

#SPEC BIOHARNESS:
#BioHarness Bluetooth Comms Link Specification 2011-11-22.pdf

# la spec bioharness
def crc_8_digest(values):
    crc = 0
    for byte in values:
        crc ^= byte
        for i in range(8):  
            if crc & 1:
                crc = (crc >> 1) ^ 0x8C
            else:
                crc = (crc >> 1)
    return crc

def enable_summary_packet(ser):
    STX = 0x02
    Packet_ID = 0xBD
    DLC = 0x02
    Payload_Upade_Period_LS = 1
    Payload_Update_Period_MS = 0
    Payload = [Payload_Upade_Period_LS, Payload_Update_Period_MS]
    CRC = crc_8_digest(Payload)
    ETX = 0x03

    message_bytes = [STX, Packet_ID, DLC]+Payload+[CRC, ETX]
    try:
        ser.write(b'\x02\xBD\x02\x01\x00\xC4\x03')
        print('')
    except:
        print('')

def lifesign_request(ser):
    STX = 0x02
    Packet_ID = 0xA4
    DLC = 0x04
    Payload = [0, 0 ,0 ,0]
    CRC = crc_8_digest(Payload)
    ETX = 0x03
    
    message_bytes = [STX, Packet_ID, DLC]+Payload+[CRC, ETX]
    try:
        ser.write(b'\x02\xa4\x04\x00\x00\x00\x00\x00\x03')
        print('')
    except:
        print('')

def extract_BH_Data(Payload):
    sequence_number = Payload[0]

    #TimeStamp
    year = Payload[1] + (Payload[2] << 8)
    month = Payload[3]
    day = Payload[4]
    day_ms = Payload[5] + (Payload[6] << 8) + (Payload[7] << 16) + (Payload[8] << 24)

    date = datetime.date(year=year, month=month, day=day)
    timestamp = time.mktime(date.timetuple()) + day_ms / 1000.0

    #HR
    HR = Payload[10] + (Payload[11] << 8)

    #VHR
    VHR = Payload[35] + (Payload[36] << 8)

    print('timestamp: '+str(timestamp)+(' HR: ')+str(HR)+(' VHR : ')+str(VHR))
    file = open("output.txt","w");
    output = str(HR);
    file.write(output);

def read_data(BH):
    Payload = []

    byte = BH.read(1)

    timeout_occured = hasattr(BH, "timeout") and not len(byte)
    if timeout_occured:
        BH.close()
        retries = 100
        for retry in range(retries):
            try:
                BH.open()
                print('reouverture de port OK')
            except:
                print('reouverture de port KO')
                time.sleep(1.0)
                continue
            break
                
    STX = 0x02
    byte = ord(chr(byte[0]))

    if byte == STX:
        ID = BH.read(1)
        ID = ord(chr(ID[0]))
        
        if ID == 0xA4:
            print(' Lifesign recue')
        if ID == 0xBD:
            print(' summary recue')

        if ID == 0x2B:
            # lecture DLC
            DLC = BH.read(1)
            DLC = ord(chr(DLC[0]))

            #lecture payload
            while len(Payload) < DLC:
                data = BH.read(1)
                Payload.append(ord(chr(data[0])))

            # calcul CRC
            CRC_from_summary = BH.read(1)
            CRC_from_summary = ord(chr(CRC_from_summary[0]))
            CRC_from_payload = crc_8_digest(Payload)
            if CRC_from_summary != CRC_from_payload:
                print("CRC different")
                pass

            # lecture fin du paquet
            Status_Dict = {0x03: "ETX", 0x06: "ACK", 0x15: "NAK"}
            End_Packet = BH.read(1)
            End_Packet = ord(chr(End_Packet[0]))
            if Status_Dict.get(End_Packet) is None:
                print("Invalide ACK byte")
                pass

            #recuperation des donnees VHR et TimeStamp
            extract_BH_Data(Payload)
         

def main():
    # Connexion au module
    BH = serial.Serial()
    BH.port = 'COM3'
    BH.baudrate = 9600
    BH.bytesize = 8
    BH.parity = 'N'
    BH.stopbits = 1
    BH.close()
    try:
        BH.open()
    except:
        print('Module non connect')

    enable_summary_packet(BH)
    lifesign_request(BH)
    

    #lecture des donnees
    RUNNING = True
    while RUNNING:
        read_data(BH)
    
if __name__ == "__main__":
    main()
