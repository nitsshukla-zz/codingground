Module test_tc_adc_temp

Public Sub tc_adc_temp
     ' //-------------------------------------------
 ' //  Declare and initialize local variables
 ' //--------------------------------------------
    Dim mystatus As Long=0
    Dim myresult As Long=0
    Dim test_result As Long
    Dim test_number As Long=1
    Dim clk_count As Long=0
    Dim num_failed_cycles As Long=0
     ' //int	trim_code=0;
    Dim trim_code As Long
    Dim trim_code_7_0 As Long
    Dim loop_indx As Long
    Dim ADC_In_V As Double
    Dim ADC_MaxV_pretrim As Double
    Dim ADC_MinV_pretrim As Double
    Dim ADC_Range_pretrim As Double
    Dim ADC_Range_delta_pretrim As Double
    Dim ADC_VCM_pretrim As Double
    Dim ADC_VCM_delta_pretrim As Double
    Dim ADC_MaxV_postrim As Double
    Dim ADC_MinV_postrim As Double
    Dim ADC_Range_postrim As Double
    Dim ADC_Range_delta_postrim As Double
    Dim ADC_VCM_postrim As Double
    Dim ADC_VCM_delta_postrim As Double
    Dim ADC_Range_Target As Double=0.803 ' //Target = (1.4V - 0.6V)*256/255 = 0.803V
    Dim ADC_VCM_Target As Double=1.0
    Dim Low_win_V As Double
    Dim High_win_V As Double
    Dim read_data As Long
 ' ///////////////////////////////PowerUp///////////////////////
    ' analog_opcode = CLng("Power_Up")
    setDCVolts("VBAT",0.0)
    setDCVolts("DVDD",0.0)
    setLogic("RESETB",CStr(1))
    Call Wait(10,"us")
    setLogic("RESETB",CStr(0))
    setLogic("ADDR1",CStr(0))
    Call Wait(10,"us")
    ' analog_opcode = CLng("AX90_PowerUp")
    setDCVolts("VBAT",4.2)
    setDCVolts("DVDD",1.8)
    Call Wait(100,"us")
    setLogic("RESETB",CStr(1))
    Call Wait(100,"us")
    ' analog_opcode = CLng("Init")
    Call setNoDrv("MCLK") ' //MCLK not driven
    Call setNoDrv("ATB1") ' //ATB1 not driven
    Call setNoDrv("ATB2") ' //ATB2 not driven
    ' i2c.cfg.reg_adr_len = 16
 ' // SW reset to the device to return register map to known state
    Call I2C_Write(default_device_id,reg_SOFTWARE_RESET,&H01)
    Call Wait(1,"ms")
 ' // Enter Global TM
    ' analog_opcode = CLng("En_GlobalTM")
    Call I2C_Write(default_device_id,reg_REV_ID,&H54)
    Call I2C_Write(default_device_id,reg_REV_ID,&H4d)
    Call Wait(1,"ms")
    ' analog_opcode = CLng("En_Global_pw")
    Call I2C_Write(default_device_id,reg_GLOBAL_ENABLE,&H01) ' // EN = 1
    Call Wait(300,"ns")
 ' ////////////////////////ADC Pre-Trim Measurments  /////////////////////////////////////////////////////
    ' analog_opcode = CLng("ADC_PreTrim")
    Call I2C_Write(default_device_id,reg_BROWNOUT_ENABLES,&H01) ' //Turn on  BDE_EN
 ' //	i2c.write(default_device_id, reg_TEST_MEAS_ADC_GAIN_TRIM, 'h00);  //Initial ADC_Gain trim code
 ' //	i2c.write(default_device_id, reg_TEST_MEAS_ADC_OFFSET_TRIM, 'h00);  //Initial ADC_Gain trim code
 ' //	i2c.write(default_device_id, reg_TEST_ATB, 'h21);  //set ATB_MEAS_ADC_TM_TEST and ATB1_EN
 ' // i2c.write(default_device_id, reg_TEST_ATB, 'h20);  //set ATB_MEAS_ADC_TM_TEST and ATB1_EN
    Call Wait(100,"us")
    'Original SystemVerilog code : fork
        ' analog_opcode = CLng("gen_bclk")
        Call runPatternFlex("patterns/bclk_gen.atp","patterns/TSB_Dig_IO.txt","","",num_failed_cycles,"patterns/bclk_gen_STCC",CStr(0))
 ' /////////ADC TEMP setup ///////////////
        ' analog_opcode = CLng("ADC_SetUp")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_BASE_DIVIDE_MSBYTE,&H00)
        Call I2C_Write(default_device_id,reg_MEAS_ADC_BASE_DIVIDE_LSBYTE,&H0b)
        ' analog_opcode = CLng("ADC_Conf")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CONFIG,&H04)
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CHAN_2_FILT_CONFIG,&H0c) ' //set MEAS_ADC_CH2_EN
        ' analog_opcode = CLng("ADC_Vptat")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_TEST_VPTAT,&H01) ' //set MEAS_ADC_VPTAT_TEST_EN and MEAS_ADC_TEMP_CHK
        ' analog_opcode = CLng("read_ADC")
        Call Wait(200,"us")
        Call I2C_Read(default_device_id,reg_MEAS_ADC_CHAN_2_READBACK,read_data)
        testLimits(985,CDbl(read_data),CDbl(40),CDbl(44),test_result)
 ' /////Check TEMP trim bit weights  //////////////
        ' analog_opcode = CLng("TEMPtrim_x07") ' //+7C
        Call I2C_Write(default_device_id,reg_TEST_MEAS_ADC_TEMP_TRIM,&H07) ' //MEAS_ADC_EN_TM = 1
        Call Wait(200,"ns")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CONFIG,&H04) ' //set MEAS_ADC_CH2_EN
        Call Wait(200,"us")
        Call I2C_Read(default_device_id,reg_MEAS_ADC_CHAN_2_READBACK,read_data)
        testLimits(990,CDbl(read_data),CDbl(46),CDbl(50),test_result)
        ' analog_opcode = CLng("TEMPtrim_x08") ' //-8C
        Call I2C_Write(default_device_id,reg_TEST_MEAS_ADC_TEMP_TRIM,&H08) ' //MEAS_ADC_EN_TM = 1
        Call Wait(200,"ns")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CONFIG,&H04) ' //set MEAS_ADC_CH2_EN
        Call Wait(200,"us")
        Call I2C_Read(default_device_id,reg_MEAS_ADC_CHAN_2_READBACK,read_data)
        testLimits(991,CDbl(read_data),CDbl(34),CDbl(38),test_result)
        ' analog_opcode = CLng("TEMPtrim_x00") ' //+0C
        Call I2C_Write(default_device_id,reg_TEST_MEAS_ADC_TEMP_TRIM,&H00) ' //MEAS_ADC_EN_TM = 1
        Call Wait(200,"ns")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CONFIG,&H04) ' //set MEAS_ADC_CH2_EN
        Call Wait(200,"us")
        Call I2C_Read(default_device_id,reg_MEAS_ADC_CHAN_2_READBACK,read_data)
        testLimits(992,CDbl(read_data),CDbl(40),CDbl(44),test_result)
        ' analog_opcode = CLng("CleanUp")
        Call I2C_Write(default_device_id,reg_MEAS_ADC_TEST_MODE_CONFIG,&H00) ' //Off
        Call I2C_Write(default_device_id,reg_MEAS_ADC_TEST_VPTAT,&H00) ' //Off
        Call I2C_Write(default_device_id,reg_MEAS_ADC_CONFIG,&H00)
        Call I2C_Write(default_device_id,reg_MEAS_ADC_BASE_DIVIDE_LSBYTE,&H00)
    'Original SystemVerilog code : join_any;
    'Original systemVerilog Code : disable fork
    Call setNoDrv("BCLK")
End Sub


End Module
