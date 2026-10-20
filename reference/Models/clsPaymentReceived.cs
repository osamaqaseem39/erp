using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proCabWeb.Models
{
    public class clsPaymentReceived
    {
        public int PAYMENT_ID { get; set; }
        public int USER_ID { get; set; }
        public int BILLING_ID { get; set; }
        public decimal? AMOUNT { get; set; }
        public string REFERENCE_ID { get; set; }
        public string RESULT_CODE { get; set; }
        public string RETURN_CODE { get; set; }
        public string RETURN_MESSAGE { get; set; }
        public string RESPONSE_CODE { get; set; }
        public string AUTH_CODE { get; set; }
        public string AVS_RESULT_CODE { get; set; }
        public string TRANS_ID { get; set; }
        public decimal? PAYMENT_RECEIVED1 { get; set; }
        public DateTime PAYMENT_RCVD_DATE { get; set; }
        public int PAYMENT_METHOD_ID { get; set; }
        public int? CREDIT_CARD_ID { get; set; }
        public string PAYMENT_REFERENCE { get; set; }
        public string API_RETURN_CODE { get; set; }
        public string API_RETURN_STATUS { get; set; }
        public string API_RETURN_MESSAGE { get; set; }
        public int? BATCH_NO { get; set; }
        public DateTime? REF_COMM_PAID_DATE { get; set; }
        public string REF_COMM_PAID { get; set; }
        public DateTime CREATION_DATE { get; set; }
        public int CREATED_BY { get; set; }
        public DateTime UPDATE_DATE { get; set; }
        public int UPDATED_BY { get; set; }
        public string MONTH { get; set; }
        public string YEAR { get; set; }
        public string CVV_CODE { get; set; }
        public string CREDIT_CARD_NO { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string ADRESS1 { get; set; }
        public string ADRESS2 { get; set; }
        public string CITY { get; set; }
        public string ZIPCODE { get; set; }
        public string CUSTOMER_PROFILE_ID { get; set; }
        public string CREDIT_CARD_TYPE { get; set; }
        public int USER_CREDIT_CARD_ID { get; set; }
        public string Payment_Method { get; set; }
        public string CUSTOMER_PAYMENT_PROFILE_ID { get; set; }
        public string PaypalEmail { get; set; }
        public string IsExpired { get; set; }
        public string PAYPAL_PAYER_ID { get; set; }
        public string SUBSCRIPTION_ID { get; set; }
        public string EMAIL { get; set; }
        public int? PAYMENT_DETAIL_ID { get; set; }
        public int? PREVIOUS_CREDIT_CARD { get; set; }
    }
}