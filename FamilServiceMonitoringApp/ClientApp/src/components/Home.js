import React, { Component } from 'react';
import { Chart } from 'primereact/chart';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormHelperText from '@material-ui/core/FormHelperText';
import FormControl from '@material-ui/core/FormControl';
import Select from '@material-ui/core/Select';

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

        this.state = {
            data: this.getData([]),
            results: [],
            excessRate: 0,
            eventsCount: 0,
            period: 'minute'
        };

        this.request();
    }

    handleChange = name => evt => {
        this.setState({ [name]: evt.target.value});
    }

    getData(result) {

        let labels = [];

        let events = [];
        let accessibility = [];
        if (result && result.events) {
            events = result.events;
            accessibility = result.accessibility;
        }

        if (events.length > 0) {
            labels.push(result.start);

            for (var i = 0; i < events.length - 2; i++) {
                labels.push("");
            }

            labels.push(result.end);
        }
        
        return {
            labels: labels,
            datasets: [{
                type: 'line',
                label: "Calculate",
                data: events,
                borderColor: '#42A5F5'
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
            body: JSON.stringify({ period: this.state.period}),
            headers: {
                'Content-Type': 'application/json',
            },
        }).then(function (res) {
            res.json().then((result) => {
                scope.setState(
                    {
                        data: scope.getData(result),
                        excessRate: result.excessRate,
                        eventsCount: result.eventsCount,
                        allEventsCount: result.allEventsCount,
                        avg: result.avg
                    });
                setTimeout(scope.request, scope.interval * 1000);
            }).catch((err) => console.log(err));
        });
    }

    render () {
        return (
            <div>
                <div>Всего запросов: {this.state.allEventsCount}</div>
                <div>Процент превышения 3с за период: {this.state.excessRate.toFixed(2) + "%"}</div>
                <div>Всего запросов за период: {this.state.eventsCount}</div>
                <div>Среднее время отклика за период: {this.state.avg + "мс"}</div>
                <FormControl>
                    <InputLabel id="demo-simple-select-helper-label">Период мониторинга</InputLabel>
                    <Select
                        labelId="demo-simple-select-helper-label"
                        id="demo-simple-select-helper"
                        value={this.state.period}
                        onChange={this.handleChange("period")}
                    >
                        <MenuItem value={"minute"}>Последняя минута</MenuItem>
                        <MenuItem value={"5minute"}>Последние 5 минут</MenuItem>
                        <MenuItem value={"hour"}>Последний час</MenuItem>
                        <MenuItem value={"day"}>Сегодняшний день</MenuItem>
                    </Select>
                </FormControl>
                <Chart ref={this.chart} type="line" data={this.state.data} options={this.options} width="100vw" height="80vh" />
            </div>
        );
    }
}
