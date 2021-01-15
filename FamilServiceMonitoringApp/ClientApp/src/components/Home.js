import React, { Component } from 'react';
import { Chart } from 'primereact/chart';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormHelperText from '@material-ui/core/FormHelperText';
import FormControl from '@material-ui/core/FormControl';
import Select from '@material-ui/core/Select';
import TextField from '@material-ui/core/TextField';

export class Home extends Component {
  static displayName = Home.name;
    chart = {};

    options = {
        animation: {
            duration: 0,
        },
        scales: {
            yAxes: [{
                ticks: {
                    suggestedMin: 10,
                    suggestedMax: 3000,
                    stepSize: 10
                }
            }]
        },
        maintainAspectRatio: false
    }

    interval = 5;

    constructor() {
        super();
        
        this.request = this.request.bind(this);
        this.getData = this.getData.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.onCustomStartChange = this.onCustomStartChange.bind(this);
        this.onCustomEndChange = this.onCustomEndChange.bind(this);

        this.state = {
            data: this.getData([]),
            results: [],
            excessRate: 0,
            eventsCount: 0,
            period: 'hour',
            avgByPeriod: 0,
            excessRateByPeriod: 0
        };

        this.request();
    }

    handleChange = name => evt => {
        this.setState({ [name]: evt.target.value });
    }

    onCustomStartChange(evt) {
        //var customStart = new Date(evt.target.value);
        //if (!this.state.customEnd) {
        //    customStart.setDate(customStart.getDate() + 1);
        //    this.setState({ customStart: evt.target.value, customEnd: evt.target.value });
        //}
        //else {
        //    this.setState({ customStart: evt.target.value });
        //}
        this.setState({ customStart: evt.target.value });
    }

    onCustomEndChange(evt) {
        this.setState({ customEnd: evt.target.value });
    }

    getData(result) {

        let labels = [];

        let events = [];
        let ciEvents = [];
        let accessibility = [];
        if (result && result.events) {
            events = result.events;
            ciEvents = result.ciEvents;
            accessibility = result.accessibility;
        }

        if (events.length > 0) {
            labels = result.labels;
            //labels.push(result.start);

            //for (var i = 0; i < events.length; i++) {
            //    labels.push(events[i].time);
            //}

            //labels.push(result.end);
        }
        
        return {
            labels: labels,
            datasets: [{
                type: 'bar',
                label: "Calculate",
                data: events,
                borderColor: '#42A5F5'
            },
            {
                type: 'bar',
                label: "ContactInfo",
                data: ciEvents,
                backgroundColor: 'red',
                borderColor: '#12A5F5'
            },
            {
                type: 'line',
                label: "Доступность",
                data: accessibility,
                fill: false,
                borderColor: '#FFA726'
            },]
        };
    }

    request() {
        var scope = this;
        var results = this.state.results.splice(0);
        if (results.length > this.period * 30 / this.interval) {
            results = results.splice(1);
        }
        else {
            results = [];
            for (var i = 0; i < this.period * 30 / this.interval; i++) results.push(0);
        }

        fetch("api/Processing/Calculate", {
            method: "POST",
            body: JSON.stringify({ period: this.state.period, start: new Date(this.state.customStart), end: new Date(this.state.customEnd) }),
            headers: {
                'Content-Type': 'application/json',
            },
        }).then(function (res) {
            res.json().then((result) => {
                scope.setState(
                    {
                        data: scope.getData(result),
                        excessRate: result.excessRate,
                        excessRateByPeriod: result.excessRateByPeriod,
                        eventsCount: result.eventsCount,
                        allEventsCount: result.allEventsCount,
                        avg: result.avg,
                        avgByPeriod: result.avgByPeriod,
                    });
                setTimeout(scope.request, scope.interval * 1000);
            }).catch((err) => console.log(err));
        });
    }

    render () {
        return (
            <div>
                <div>Всего запросов, шт: {this.state.allEventsCount}</div>
                <div>Среднее время отклика, мс: {this.state.avg + "мс"}</div>
                <div>Общий процент превышения 3с, %: {this.state.excessRate.toFixed(2) + "%"}</div>

                <FormControl>
                    <InputLabel id="demo-simple-select-helper-label">Период мониторинга</InputLabel>
                    <Select
                        labelId="demo-simple-select-helper-label"
                        id="demo-simple-select-helper"
                        value={this.state.period}
                        onChange={this.handleChange("period")}
                    >
                        <MenuItem value={"hour"}>Последний час</MenuItem>
                        <MenuItem value={"day"}>Последние сутки</MenuItem>
                        <MenuItem value={"custom"}>Произвольный период</MenuItem>
                    </Select>
                </FormControl>
                {this.state.period === "custom" && <div>
                    <TextField
                        id="datetime-local-custom-start"
                        label="С"
                        type="datetime-local"
                        value={this.state.customStart}
                        onChange={this.onCustomStartChange}
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                    <TextField
                        id="datetime-local-custom-end"
                        label="По"
                        type="datetime-local"
                        value={this.state.customEnd}
                        onChange={this.onCustomEndChange}
                        InputLabelProps={{
                            shrink: true,
                        }}
                    />
                </div>}

                <div>Кол-во запросов за период, шт: {this.state.eventsCount}</div>
                <div>Среднее время отклика за период, мс: {this.state.avgByPeriod + "мс"}</div>
                <div>Процент превышения 3с за период, %: {this.state.excessRateByPeriod.toFixed(2) + "%"}</div>
                <Chart ref={this.chart} type="bar" data={this.state.data} options={this.options} width="100vw" height="80vh" />
            </div>
        );
    }
}
