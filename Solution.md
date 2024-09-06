
# Intro

A full example of the current configuration file designed is provided at the bottom. 

This config more closely matches the requirements in the ReadMe file but in building it I realized that there is another schema that might be more useful in some cases.

This alternate configuration example is also provided below. 
That second version would allow for a different implementation that would require less rules per resource/endpoint as 
it allow for multiple entries in filtering/targeting requests/users.

## Methodology

The solution provided was intended to be memory efficient. As such, some of the functionality one could build that required storing separate requests per resource
or per user was not included.

Naturally, the added functionality would have made for a larger task.

### Configuration File 
The configuration file is a JSON file that contains the rules for rate limiting. 

The configuration file is loaded by the `Limiter` class and used to determine the rate limits for each API resource.

The configuration file contains an array of 'Rule' instances. 

Each rule has a name, a match object, a conditions array, and a limits array.

## Limiter Class Usage

The `Limiter.ApplyRateLimit` method is used to apply the rate limits to a request. 

The ``ApplyRateLimit`` takes in the following parameters:
- the API URL
- access token
- Headers

The API URL and Headers will be used to find a matching `Rule` in the configuration file.

### Default Rule

If no matching rule is found, the default `Rule` is used if there is one present. 

The `Match`, `Conditions` and `Limits` properties behave the same as they do on any other `Rule object`

> The Rule is considered to be the default if the `Rule.IsDefault` is set to `True`

## No Rule Found?

If there no matching `Rule` or default `Rule` found, then ``ApplyRateLimit`` will simply return a Success result intended to allow the request through.

## Rule Class

Each `Rule` class will first try to match the API URL against the `Match` object's `ApiUrl` property which is intended to be a regex pattern.

> Regex Patterns are used by the Match and Conditions objects to allow for greater flexibility.

Once a match is found, the `Conditions` array is checked to see if the request meets all the conditions. 
If all conditions are met, the `Limits` array is used to determine if the request should be allowed or not.

If any of the conditions are not met, the 'Rule' is disqualified from selection.

## Limit Class

The Limit.LimitType property is used to determine the type of limit that is being applied.

#### LimitType.TimeWindow

The LimitType.TimeWindow is used to limit the number of requests that can be made within a certain time span as defined by `WindowType` property.

The WindowType allows for the following values:
- Second
- Minute
- Hour 
- Day 
- Week
- Month

In combination with `RequestLimit`, you can indicate how many requests are allowed within the specified time span.

The time spans are not sliding. They are fixed time spans that start at the first request and end at the end of the time span.

```json
{
    "type": "timeWindow",
    "requestLimit": 60,
    "windowType": "minute"
}
```

#### LimitType.RequestSpacing

The LimitType.RequestSpacing is used to limit the number of requests that can be made within a certain time span as defined by `Spacing` property.

Below is an example that limits requests to 1 per 500 milliseconds.
```json
{
    "type": "requestSpacing",
    "spacing": "00:00:00.500"
}
```



# Full Config Example

```json
{
  "limiter": {
    "rules": [
      {
        "name": "Users Rule",
        "isDefault": false,
        "match": {
          "apiUrl": "/api/users"
        },
        "conditions": [
          {
            "input": "HTTP_USER_AGENT",
            "pattern": "Mobile"
          }

        ],
        "limits": [
          {
            "type": "timeWindow",
            "requestLimit": 60,
            "windowType": "minute"
          },
          {
            "type": "timeWindow",
            "requestLimit": 70,
            "windowType": "hour"
          },
          {
            "type": "requestSpacing",
            "spacing": "00:00:00.500"
          }
        ]
      },
    
      {
        "name": "Default Rule",
        "isDefault": true,
        "limits": [
          {
            "type": "timeWindow",
            "requestLimit": 60,
            "windowType": "minute"
          },
          {
            "type": "timeWindow",
            "requestLimit": 70,
            "windowType": "hour"
          },
          {
            "type": "requestSpacing",
            "spacing": "00:00:00.500"
          }
        ]
      }
    ]
  }
}

```
# Alternate Config Example

This structure allows an Array of Conditions within 1 Api Url.

Within these Conditions, you Matching Headers are associated with their own distinct Limit arrays.

This allows us to define 1 Rule for 1 Api end point that will contain N number of different MatchingHeaders (target users) with their own Limits

```json
{
    "limiter": {
        "rules": [
            {
                "name": "Users Rule",
                "isDefault": false,
                "match": {
                    "apiUrl": "/api/users",
                    "conditions": [
                        {
                            "matchingHeaders": [
                                {
                                    "input": "HTTP_USER_AGENT",
                                    "pattern": "Mobile"
                                }
                            ],
                            "limits": [
                                {
                                    "type": "timeWindow",
                                    "requestLimit": 60,
                                    "windowType": "minute"
                                },
                                {
                                    "type": "timeWindow",
                                    "requestLimit": 70,
                                    "windowType": "hour"
                                },
                                {
                                    "type": "requestSpacing",
                                    "spacing": "00:00:00.500"
                                }
                            ]
                        }
                    ]
                }
            }
        ]
    }
}
```



