# Fingerprint Verification API  

A simple C# API for fingerprint verification using **SourceAFIS**. This API takes two fingerprint image URLs, compares them, and returns whether they match.  

## Features  

- Accepts saved and scanned fingerprint image URLs  
- Uses **SourceAFIS** for fingerprint matching  
- Returns a match status and similarity score  

## Technologies Used  

- C#  
- ASP.NET Core  
- SourceAFIS  

## API Endpoints  

### Verify Fingerprint  

#### **Endpoint:**  
```http
POST /api/fingerprint/verify```

### Request Body:
```{
  "savedFingerUrl": "https://example.com/saved_fingerprint.png",
  "userFingerUrl": "https://example.com/scanned_fingerprint.png"
}```

### Response
```{
  "isMatch": true,
  "score": 58,
  "message": "Fingerprint matched"
}```
